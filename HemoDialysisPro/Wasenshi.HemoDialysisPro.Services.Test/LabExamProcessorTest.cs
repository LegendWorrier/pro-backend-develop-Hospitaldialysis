using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    public class LabExamProcessorTest
    {
        private IFixture _fixture;

        private List<LabExam> labExams;

        private TimeZoneInfo tz;

        public LabExamProcessorTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            bool circleCi = Environment.GetEnvironmentVariable("CIRCLECI") != null;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { KeyValuePair.Create("TIMEZONE", circleCi ? "TH" : "SE Asia Standard Time") }) // + 7.00
                .Build();
            _fixture.Inject<IConfiguration>(config);

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));

            labExams = new List<LabExam>
            {
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Albumin" },
                    EntryTime = DateTime.UtcNow.AddDays(-3),
                    LabValue = 10.5f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Heparin" },
                    EntryTime = DateTime.UtcNow.AddDays(-3),
                    LabValue = 30f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Albumin" },
                    EntryTime = DateTime.UtcNow.AddDays(-10),
                    LabValue = 5f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Albumin" },
                    EntryTime = DateTime.UtcNow.AddDays(-33),
                    LabValue = 12.5f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Heparin" },
                    EntryTime = DateTime.UtcNow.AddDays(-5),
                    LabValue = 20f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Heparin" },
                    EntryTime = DateTime.UtcNow.AddDays(-8),
                    LabValue = 20.5f
                },
                new LabExam
                {
                    LabItem = new LabExamItem { Name = "Calcium" },
                    EntryTime = DateTime.UtcNow.AddDays(-10),
                    LabValue = 22f
                }
            };
        }

        [Fact]
        public void Processor_Should_Process_Correctly()
        {
            var sub = _fixture.Create<LabExamProcessor>();
            var result = sub.ProcessData(labExams);

            result.Columns.Should().BeInDescendingOrder().And.HaveCount(5);
            result.Data.Should().HaveCount(3).And.BeInAscendingOrder(x => x.Key.Name, "Data should also order by Name Asc");
            var firstRecord = result.Data.First();
            firstRecord.Value[0][0].LabValue.Should().Be(10.5f);
            firstRecord.Value[1].Count.Should().Be(0, "the value should be null");
            firstRecord.Value[2].Count.Should().Be(0, "the value should be null");
            firstRecord.Value[3][0].LabValue.Should().Be(5f);
            firstRecord.Value[4][0].LabValue.Should().Be(12.5f);

            var secondRecord = result.Data.Skip(1).First();
            secondRecord.Value[0].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[1].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[2].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[3][0].LabValue.Should().Be(22f);
            secondRecord.Value[4].Count.Should().Be(0, "the value should be null");

            var thirdRecord = result.Data.Last();
            thirdRecord.Value[0][0].LabValue.Should().Be(30f);
            thirdRecord.Value[1][0].LabValue.Should().Be(20f);
            thirdRecord.Value[2][0].LabValue.Should().Be(20.5f);
            thirdRecord.Value[3].Count.Should().Be(0, "the value should be null");
            thirdRecord.Value[4].Count.Should().Be(0, "the value should be null");
        }

        //[Fact]
        //public async Task Generate_Excel_Should_Work_As_Expected()
        //{
        //    var sub = _fixture.Create<LabExamProcessor>();
        //    var data = sub.ProcessData(labExams);
        //    data.Patient = new Patient
        //    {
        //        Name = "Sam Brian",
        //        Gender = "M",
        //        BirthDate = DateTime.UtcNow.AddYears(-20)
        //    };
        //    var fileData = await sub.WriteToExcel(data);
        //    await File.WriteAllBytesAsync("tmp.xlsx", fileData);

        //    var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

        //    using (var reader = new ExcelReader("tmp.xlsx"))
        //    {
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Lab Examination");
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Patient's Name:");
        //        reader.GetField(1).Should().Be(data.Patient.Name);
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Patient's Age:");
        //        reader.GetField(1).Should().Be("20");
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Patient's Gender:");
        //        reader.GetField(1).Should().Be("Male");
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Timezone:");
        //        reader.GetField(1).Should().Be(tz.DisplayName);

        //        await reader.ReadAsync();
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Name/Date");
        //        reader.GetField(1).Should().Be("Reference");
        //        reader.GetField(2).Should().Be($"{tzNow.AddDays(-3).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        //        await reader.ReadAsync();
        //        reader.GetField(0).Should().Be("Albumin");
        //    }

        //    File.Delete("tmp.xlsx");
        //}

        [Fact]
        public void Processor_Should_Process_WithTimeZone_Correctly()
        {
            foreach (var item in labExams)
            {
                var dtOffset = TimeZoneInfo.ConvertTime(new DateTimeOffset(item.EntryTime, TimeSpan.Zero), tz);
                item.EntryTime = dtOffset.AddTicks(-dtOffset.TimeOfDay.Ticks).AddHours(2).ToUniversalTime().DateTime; // set time to 2 am , entryTimes will be pushed back by 1 day in UTC
            }

            var sub = _fixture.Create<LabExamProcessor>();
            var result = sub.ProcessData(labExams);
            result.Columns.Should().BeInDescendingOrder().And.HaveCount(5);
            // date columns should still be correct (no offset by 1 day)
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
            var todayByTz = TimeZoneInfo.ConvertTimeToUtc(tzNow.Date, tz);
            result.Columns.Should().ContainInOrder(todayByTz.AddDays(-3), todayByTz.AddDays(-5), todayByTz.AddDays(-8), todayByTz.AddDays(-10), todayByTz.AddDays(-33));

            result.Data.Should().HaveCount(3).And.BeInAscendingOrder(x => x.Key.Name, "Data should also order by Name Asc");
            var firstRecord = result.Data.First();
            firstRecord.Value[0][0].LabValue.Should().Be(10.5f, "should have value");
            firstRecord.Value[1].Count.Should().Be(0, "the value should be null");
            firstRecord.Value[2].Count.Should().Be(0, "the value should be null");
            firstRecord.Value[3][0].LabValue.Should().Be(5f, "should have value");
            firstRecord.Value[4][0].LabValue.Should().Be(12.5f, "should have value");

            var secondRecord = result.Data.Skip(1).First();
            secondRecord.Value[0].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[1].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[2].Count.Should().Be(0, "the value should be null");
            secondRecord.Value[3][0].LabValue.Should().Be(22f, "should have value");
            secondRecord.Value[4].Count.Should().Be(0, "the value should be null");

            var thirdRecord = result.Data.Last();
            thirdRecord.Value[0][0].LabValue.Should().Be(30f, "should have value");
            thirdRecord.Value[1][0].LabValue.Should().Be(20f, "should have value");
            thirdRecord.Value[2][0].LabValue.Should().Be(20.5f, "should have value");
            thirdRecord.Value[3].Count.Should().Be(0, "the value should be null");
            thirdRecord.Value[4].Count.Should().Be(0, "the value should be null");
        }
    }
}
