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
    public class LabExamStatTest
    {
        private readonly IFixture _fixture;

        private readonly List<LabExamItem> labItems;
        private readonly List<LabExam> labList;

        private readonly TimeZoneInfo tz;

        public LabExamStatTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            bool circleCi = Environment.GetEnvironmentVariable("CIRCLECI") != null;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { KeyValuePair.Create("TIMEZONE", circleCi ? "TH" : "SE Asia Standard Time") }) // + 7.00
                .Build();
            _fixture.Inject<IConfiguration>(config);
            _fixture.Freeze<IStatProcessor>(c => c.FromFactory(() => c.Create<StatProcessor>()));

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            labItems = new List<LabExamItem>
            {
                new()
                {
                    Id = 1,
                    Name = "Ht"
                },
                new LabExamItem()
                {
                    Id =2,
                    Name = "Ca"
                }
            };

            var targetTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set time to 2 am of 1st day of the month
            labList = new List<LabExam>
            {
                new LabExam
                {
                    Id = Guid.NewGuid(),
                    EntryTime = targetTime,
                    LabItem = labItems[0],
                    LabItemId = labItems[0].Id,
                    LabValue = 10,
                    Created = targetTime,
                },
                new LabExam
                {
                    Id = Guid.NewGuid(),
                    EntryTime = targetTime.AddDays(5),
                    LabItem = labItems[1],
                    LabItemId =labItems[1].Id,
                    LabValue = 20,
                    Created = targetTime.AddDays(5)
                }
            };

            var labRepo = _fixture.Freeze<Moq.Mock<ILabExamRepository>>();
            labRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(labList.AsQueryable());
            var labUow = _fixture.Freeze<Moq.Mock<ILabUnitOfWork>>();
            labUow.SetupGet(x => x.LabExam).Returns(labRepo.Object);
        }

        [Fact]
        public void LabExam_Stat_Should_Process_Correctly()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // supposely just within range for default 1 record in test data
            var targetFilter = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 0, 0, 0, tz.BaseUtcOffset);
            var targetInterval = targetFilter.AddMonths(1) - targetFilter;

            var sub = _fixture.Create<LabExamStat>();
            var result = sub.GetLabExamStat("M", targetFilter.ToUtcDate());

            result.Columns.Count().Should().Be((int)targetInterval.TotalDays);

            var ht = result.Rows.FirstOrDefault(x =>
            x.Title.Contains("Ht", StringComparison.OrdinalIgnoreCase));
            ht.Should().NotBeNull();
            ht.Data[0].Should().BeOfType<StatInfo>();
            ht.Data[0].Avg.Should().Be(10);

            var ca = result.Rows.FirstOrDefault(x =>
            x.Title.Contains("Ca", StringComparison.OrdinalIgnoreCase));
            ca.Should().NotBeNull();
            ca.Data[5].Should().BeOfType<StatInfo>();
            ca.Data[5].Avg.Should().Be(20);
        }
    }
}
