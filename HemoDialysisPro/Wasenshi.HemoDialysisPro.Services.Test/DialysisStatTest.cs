using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Xunit;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    public class DialysisStatTest
    {
        private readonly IFixture _fixture;

        private readonly List<HemodialysisRecord> hemoList;

        private readonly TimeZoneInfo tz;

        public DialysisStatTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            bool circleCi = Environment.GetEnvironmentVariable("CIRCLECI") != null;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { KeyValuePair.Create("TIMEZONE", circleCi ? "TH" : "SE Asia Standard Time") }) // + 7.00
                .Build();
            _fixture.Inject<IConfiguration>(config);

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            _fixture.Freeze<IStatProcessor>(c => c.FromFactory(() => c.Create<StatProcessor>()));

            var targetTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set time to 2 am of 1st day of the month
            hemoList = new List<HemodialysisRecord>
            {
                new HemodialysisRecord
                {
                    Id = Guid.NewGuid(),
                    DialysisPrescription = new()
                    {
                        DryWeight = 50
                    },
                    Dehydration = new DehydrationRecord()
                    {
                        LastPostWeight = 49,
                        PreTotalWeight = 53
                    },
                    Created = targetTime,
                    CompletedTime = targetTime.AddHours(1) // need to be completed first
                },
                new HemodialysisRecord
                {
                    Id = Guid.NewGuid(),
                    DialysisPrescription = new()
                    {
                        DryWeight = 60
                    },
                    Dehydration = new DehydrationRecord()
                    {
                        LastPostWeight = 59,
                        PreTotalWeight = 65
                    },
                    PreVitalsign = new List<VitalSignRecord>()
                    {
                        new()
                        {
                            BPD = 62,
                            BPS = 120
                        }
                    },
                    Created = targetTime.AddDays(5),
                    CompletedTime = targetTime.AddDays(5).AddHours(1) // need to be completed first
                }
            };

            var hemoRepo = _fixture.Freeze<Moq.Mock<IHemoRecordRepository>>();
            hemoRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(hemoList.AsQueryable());
            hemoRepo.Setup(x => x.GetAllWithPatient(Moq.It.IsAny<bool>())).Returns(hemoList.Select(x => new HemoRecordResult
            {
                Patient = new Patient
                {
                    Id = x.PatientId
                },
                Record = x,
            }).AsQueryable());

            var hemoUnit = _fixture.Freeze<Moq.Mock<IHemoUnitOfWork>>();
            hemoUnit.SetupGet(x => x.HemoRecord).Returns(hemoRepo.Object);
        }

        [Fact]
        public void Dialysis_Stat_Should_Process_Correctly()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // supposely just within range for default 1 record in test data
            var targetFilter = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 0, 0, 0, tz.BaseUtcOffset);
            var targetInterval = targetFilter.AddMonths(1) - targetFilter;

            var sub = _fixture.Create<DialysisStat>();
            var result = sub.GetDialysistStat("M", null, targetFilter.ToUtcDate());

            result.Columns.Count().Should().Be((int)targetInterval.TotalDays);

            var preWRow = result.Rows.FirstOrDefault(x =>
                x.Title.Contains("pre", StringComparison.OrdinalIgnoreCase) &&
                x.Title.Contains("weight", StringComparison.OrdinalIgnoreCase));
            preWRow.Should().NotBeNull();
            preWRow.Data[0].Should().BeOfType<StatInfo>();
            preWRow.Data[0].Avg.Should().Be(53);
            preWRow.Data[5].Should().BeOfType<StatInfo>();
            preWRow.Data[5].Avg.Should().Be(65);

            var dryWRow = result.Rows.FirstOrDefault(x =>
                x.Title.Contains("dry", StringComparison.OrdinalIgnoreCase) &&
                x.Title.Contains("weight", StringComparison.OrdinalIgnoreCase));
            dryWRow.Should().NotBeNull();
            dryWRow.Data[0].Should().BeOfType<StatInfo>();
            dryWRow.Data[0].Avg.Should().Be(50);
            dryWRow.Data[5].Should().BeOfType<StatInfo>();
            dryWRow.Data[5].Avg.Should().Be(60);

            var preBpsRow = result.Rows.FirstOrDefault(x =>
                x.Title.Contains("pre", StringComparison.OrdinalIgnoreCase) &&
                x.Title.Contains("BPS", StringComparison.OrdinalIgnoreCase));
            preBpsRow.Should().NotBeNull();
            preBpsRow.Data[0].Should().BeOfType<StatInfo>();
            preBpsRow.Data[0].Avg.Should().Be(0);
            preBpsRow.Data[5].Should().BeOfType<StatInfo>();
            preBpsRow.Data[5].Avg.Should().Be(120);

            var preBpdRow = result.Rows.FirstOrDefault(x =>
                x.Title.Contains("pre", StringComparison.OrdinalIgnoreCase) &&
                x.Title.Contains("BPD", StringComparison.OrdinalIgnoreCase));
            preBpdRow.Should().NotBeNull();
            preBpdRow.Data[0].Should().BeOfType<StatInfo>();
            preBpdRow.Data[0].Avg.Should().Be(0);
            preBpdRow.Data[5].Should().BeOfType<StatInfo>();
            preBpdRow.Data[5].Avg.Should().Be(62);
        }
    }
}
