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
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Xunit;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    /// <summary>
    /// This tests group depends on fundamental stat : AssessmentStat
    /// </summary>
    public class StatServiceTest
    {
        private IFixture _fixture;

        private List<Assessment> assessments;
        private List<AssessmentOption> options;

        private List<AssessmentItem> items;
        private List<HemodialysisRecord> hemoList;

        private readonly TimeZoneInfo tz;

        public StatServiceTest()
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
        public void Stat_Service_Should_Process_Correctly()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // test default case : 1 month (current month)
            // (should reference from current month back for 1 month)
            var sub = _fixture.Create<StatService>();
            var result = sub.GetAssessmentStat("M");

            var columnCount = DateTime.DaysInMonth(tzNow.Year, tzNow.Month);
            result.Columns.Count().Should().Be(columnCount);
            int indexAtData = columnCount - tzNow.Day;
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[indexAtData].Should().Be(1);

            result.Rows.ElementAt(2).InfoRef.Should().Be(0);
            result.Rows.ElementAt(2).Title.Should().Be("Headache");
            result.Rows.ElementAt(2).Data[indexAtData].Should().Be(1);

            // test case : 6 months
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).AddMonths(-5).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("6M");

            result.Columns.Count().Should().Be(6);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 1 year (should reference from current month back 12 months)
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).AddMonths(-11).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("Y");

            result.Columns.Count().Should().Be(12);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 2 years (should reference from current month back 24 months)
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 2, 0, 0, tzNow.Offset).AddMonths(-23).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("2Y");

            result.Columns.Count().Should().Be(24);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 5 years
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year - 4, 1, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("5Y");

            result.Columns.Count().Should().Be(5);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 10 days
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, tzNow.Month, tzNow.Day, 2, 0, 0, tzNow.Offset).AddDays(-9).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("10D");

            result.Columns.Count().Should().Be(10);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);
        }

        [Fact]
        public void Stat_Service_With_PointOfTime_Should_Process_Correctly()
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, 3, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime;

            // test default case : 1 month (March)
            var sub = _fixture.Create<StatService>();
            var result = sub.GetAssessmentStat("M", new DateTimeOffset(tzNow.Year, 3, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(31);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            result.Rows.ElementAt(2).InfoRef.Should().Be(0);
            result.Rows.ElementAt(2).Title.Should().Be("Headache");
            result.Rows.ElementAt(2).Data[0].Should().Be(1);
            // test case : 1 month April
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, 4, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("M", new DateTimeOffset(tzNow.Year, 4, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(30);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);
            // test case : 1 month Feb
            hemoList[0].CompletedTime = new DateTimeOffset(1995, 2, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("M", new DateTimeOffset(1995, 2, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(28);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 6 months
            hemoList[0].CompletedTime = new DateTimeOffset(1995, 5, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set to 5th month
            result = sub.GetAssessmentStat("6M", new DateTimeOffset(1995, 1, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime); // search from 1st month

            result.Columns.Count().Should().Be(6);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[4].Should().Be(1);

            // test case : 1 year
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year - 2, 5, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set to 5th month of 2 year before
            result = sub.GetAssessmentStat("Y", new DateTimeOffset(tzNow.Year - 2, 1, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(12);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[4].Should().Be(1);

            // test case : 10 and 20 years
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year - 10, 1, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime; // set to 1st month of 10-year before
            hemoList.Add(new HemodialysisRecord
            {
                Id = Guid.NewGuid(),
                CompletedTime = new DateTimeOffset(tzNow.Year - 20, 5, 1, 2, 0, 0, tzNow.Offset).ToUniversalTime().DateTime, // set to 5th month of 20-year before
            });
            items.Add(
                new AssessmentItem
                {
                    HemosheetId = hemoList[1].Id,
                    AssessmentId = -21L,
                    Selected = new[] { -95L, -93L }
                }
            );

            // filter from 10-year before
            result = sub.GetAssessmentStat("10Y", new DateTimeOffset(tzNow.Year - 10, 1, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(10);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // filter from 20-year before, so should find both 2 records
            result = sub.GetAssessmentStat("20Y", new DateTimeOffset(tzNow.Year - 20, 1, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(20);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);
            result.Rows.ElementAt(0).Data[10].Should().Be(1);

            // filter from 20-year before, but 10 years, so should find only 1 record
            result = sub.GetAssessmentStat("10Y", new DateTimeOffset(tzNow.Year - 20, 1, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(10);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[0].Should().Be(1);

            // test case : 10 days
            hemoList[0].CompletedTime = new DateTimeOffset(tzNow.Year, 3, 10, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime;
            result = sub.GetAssessmentStat("10D", new DateTimeOffset(tzNow.Year, 3, 1, 0, 0, 0, tzNow.Offset).ToUniversalTime().DateTime);

            result.Columns.Count().Should().Be(10);
            result.Rows.ElementAt(0).InfoRef.Should().Be(0);
            result.Rows.ElementAt(0).Title.Should().Be("Hypo-tension");
            result.Rows.ElementAt(0).Data[9].Should().Be(1);
        }
    }
}
