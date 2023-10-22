using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Xunit;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    public class AssessmentStatTest
    {
        private readonly IFixture _fixture;

        private readonly List<Assessment> assessments;
        private readonly List<AssessmentOption> options;

        private readonly List<AssessmentItem> items;
        private readonly List<HemodialysisRecord> hemoList;

        private readonly TimeZoneInfo tz;

        public AssessmentStatTest()
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

            assessments = new List<Assessment>
            {
                new Assessment
                {
                    Id = -17,
                    Order = 16,
                    Type = AssessmentTypes.Pre,
                    Name = "thrill",
                    DisplayName = "Thrill",
                    OptionType = OptionTypes.Checkbox
                },
                new Assessment
                {
                    Id = -18,
                    Order = 17,
                    Type = AssessmentTypes.Pre,
                    Name = "bruit",
                    DisplayName = "Bruit",
                    OptionType = OptionTypes.Checkbox,
                    Multi = true
                },
                new Assessment
                {
                    Id = -21,
                    Order = 0,
                    Type = AssessmentTypes.Post,
                    Name = "complication",
                    DisplayName = "Complication",
                    OptionType = OptionTypes.Checkbox,
                    Multi = true
                },
                new Assessment
                {
                    Id = -22,
                    Order = 1,
                    Type = AssessmentTypes.Post,
                    Name = "result",
                    DisplayName = "HD Result OK",
                    OptionType = OptionTypes.Checkbox
                }
            };
            options = new List<AssessmentOption>
            {
                new AssessmentOption
                {
                    Id = -96L,
                    AssessmentId = -21L,
                    Name = "no",
                    DisplayName = "No complication"
                },
                new AssessmentOption
                {
                    Id = -95L,
                    AssessmentId = -21L,
                    Name = "hypo",
                    DisplayName = "Hypo-tension"
                },
                new AssessmentOption
                {
                    Id = -94L,
                    AssessmentId = -21L,
                    Name = "muscle",
                    DisplayName = "Muscle cramp"
                },
                new AssessmentOption
                {
                    Id = -93L,
                    AssessmentId = -21L,
                    Name = "head",
                    DisplayName = "Headache"
                },
                new AssessmentOption
                {
                    Id = -99L,
                    AssessmentId = -18L,
                    Name = "continue",
                    DisplayName = "Continue"
                },
                new AssessmentOption
                {
                    Id = -98L,
                    AssessmentId = -18L,
                    Name = "systolic",
                    DisplayName = "Systolic"
                }
            };

            var targetTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set time to 2 am of 1st day of the month
            hemoList = new List<HemodialysisRecord>
            {
                new HemodialysisRecord
                {
                    Id = Guid.NewGuid(),
                    PatientId = "patient",
                    Created = targetTime,
                    CompletedTime = targetTime.AddHours(1) // need to be completed first
                }
            };
            items = new List<AssessmentItem>
            {
                new AssessmentItem
                {
                    HemosheetId = hemoList[0].Id,
                    AssessmentId = -21L,
                    Selected = new []{ -95L, -93L }
                }
            };

            var assessmentRepo = _fixture.Freeze<Moq.Mock<IAssessmentRepository>>();
            assessmentRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(assessments
                .Select(x => joinOption(x))
                .AsQueryable());

            Assessment joinOption(Assessment a)
            {
                a.OptionsList = options.Where(y => y.AssessmentId == a.Id).ToList();
                return a;
            }

            var assessmentItemRepo = _fixture.Freeze<Moq.Mock<IAssessmentItemRepository>>();
            assessmentItemRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(items.AsQueryable());

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

            _fixture.Register<IAssessmentStat>(_fixture.Create<AssessmentStat>);

            _fixture.Freeze<Moq.Mock<IMapper>>()
                .Setup(x => x.Map<AssessmentInfo>(Moq.It.IsAny<Assessment>()))
                .Returns((Assessment a) =>
                {
                    return new AssessmentInfo
                    {
                        Id = a.Id,
                        DisplayName = a.DisplayName,
                        Name = a.Name,
                        Order = a.Order
                    };
                });
        }

        [Fact]
        public void Assessment_Stat_Should_Process_Correctly()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // add more check
            var targetTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 5, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set time to 2 am of 5th day of the month
            hemoList.Add(new HemodialysisRecord
                {
                    Id = Guid.NewGuid(),
                    Created = targetTime,
                    CompletedTime = targetTime.AddHours(1) // need to be completed first
                });
            items.Add(new AssessmentItem
            {
                HemosheetId = hemoList[1].Id,
                AssessmentId = -22L, // test single type
                Checked = true
            });

            // supposely just within range for default 1 record in test data
            var targetFilter = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 0, 0, 0, tz.BaseUtcOffset);
            var targetInterval = targetFilter.AddMonths(1) - targetFilter;

            var sub = _fixture.Create<AssessmentStat>();
            var result = sub.GetAssessmentStat("M", null, targetFilter.ToUtcDate());

            result.Info.Should().HaveCount(2);
            result.Info.ElementAt(0).As<AssessmentInfo>().Id.Should().Be(-21L);

            result.Columns.Count().Should().Be((int)targetInterval.TotalDays);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            result.Rows.ElementAt(2).InfoRef.Should().Be(0);
            result.Rows.ElementAt(2).Title.Should().Be("Headache");
            result.Rows.ElementAt(2).Data[0].Should().Be(1);

            result.Rows.ElementAt(3).InfoRef.Should().Be(1);
            result.Rows.ElementAt(3).Title.Should().Be("HD Result OK");
            result.Rows.ElementAt(3).Data[4].Should().Be(1);
        }

        [Fact]
        public void Assessment_Stat_Should_AutoIgnore_No_Keyword()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // test default case : 1 month (current month)
            var sub = _fixture.Create<StatService>();
            var result = sub.GetAssessmentStat("M");

            result.Columns.Count().Should().Be(DateTime.DaysInMonth(tzNow.Year, tzNow.Month));
            result.Rows.Count().Should().Be(3);
            result.Rows.Should().NotContain(x => x.Title == "No complication");
        }
    }
}
