using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Services.Test
{
    public class SystemBoundLabProcessorTest
    {
        private IFixture _fixture;

        private List<LabExam> labExams;
        private List<LabExamItem> labItems;

        private string patientId;
        private Moq.Mock<ILabExamRepository> labRepo;
        private TimeZoneInfo tz;

        public SystemBoundLabProcessorTest()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            bool circleCi = Environment.GetEnvironmentVariable("CIRCLECI") != null;

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new[] { KeyValuePair.Create("TIMEZONE", circleCi ? "TH" : "SE Asia Standard Time") }) // + 7.00
                .Build();
            _fixture.Inject<IConfiguration>(config);
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            patientId = _fixture.Create<string>();

            labItems = new List<LabExamItem>
            {
                new LabExamItem
                {
                    Bound = Models.Enums.SpecialLabItem.BUN,
                    IsSystemBound = true,
                    Id = -1
                },
                new LabExamItem
                {
                    Bound = Models.Enums.SpecialLabItem.KtV,
                    IsSystemBound = true,
                    IsCalculated = true,
                    Id = -2
                },
                new LabExamItem
                {
                    Bound = Models.Enums.SpecialLabItem.URR,
                    IsSystemBound = true,
                    IsCalculated = true,
                    Id = -3
                },
            };

            labExams = new List<LabExam>
            {
                new LabExam
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    LabItem = new LabExamItem { Name = "BUN", Id = -1 },
                    LabItemId = -1,
                    EntryTime = tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddHours(2).AddDays(-1).ToUniversalTime().DateTime, // set time to 2 am yesterday
                    LabValue = 10.5f
                },
                new LabExam
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    LabItem = new LabExamItem { Name = "BUN", Id = -1 },
                    LabItemId = -1,
                    EntryTime = tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddHours(22).AddDays(-1).ToUniversalTime().DateTime, // set time to 10 pm yesterday
                    LabValue = 16.5f
                }
            };

            var labItemRepo = _fixture.Freeze<Moq.Mock<IRepository<LabExamItem, int>>>();
            labItemRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(labItems.AsQueryable());
            labItemRepo.Setup(x => x.Find(Moq.It.IsAny<Expression<Func<LabExamItem, bool>>>(), Moq.It.IsAny<bool>()))
                .Returns((Expression<Func<LabExamItem, bool>> where, bool include) => labItems.AsQueryable().Where(where));

            labRepo = _fixture.Freeze<Moq.Mock<ILabExamRepository>>();
            labRepo.Setup(x => x.GetAll(Moq.It.IsAny<bool>())).Returns(labExams.AsQueryable());

            var labUow = _fixture.Freeze<Moq.Mock<ILabUnitOfWork>>();
            labUow.SetupGet(x => x.LabMaster).Returns(labItemRepo.Object);
            labUow.SetupGet(x => x.LabExam).Returns(labRepo.Object);

            labRepo.Setup(x => x.Insert(Moq.It.IsAny<LabExam>()));
        }

        [Fact]
        public void ProcessBUN_LabExam_Should_Process_WithTimeZone_Correctly()
        {
            var sub = _fixture.Create<SystemBoundLabProcessor>();
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
            var lab = new LabExam
            {
                PatientId = patientId,
                LabItem = new LabExamItem { Name = "BUN", Id = -1 },
                EntryTime = tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddHours(2).ToUniversalTime().DateTime, // set time to 2am
                LabValue = 12.5f
            };
            // new value dif from last value only 4 hours (which mean: without timezone, it should fail to process)

            Action action = () => sub.ProcessBUN(lab, false);
            action.Should().NotThrow("Should process correctly, and shouldn't block because of different date");
        }

        [Fact]
        public void ProcessBUN_MoreThan2_Should_Fail()
        {
            var sub = _fixture.Create<SystemBoundLabProcessor>();
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            var lab = new LabExam
            {
                PatientId = patientId,
                LabItem = new LabExamItem { Name = "BUN", Id = -1 },
                EntryTime = tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddHours(10).AddDays(-1).ToUniversalTime().DateTime, // set date to yesterday, as same as test data
                LabValue = 12.5f
            };


            Action action = () => sub.ProcessBUN(lab, false);
            action.Should().ThrowExactly<SystemBoundException>("Should block more than 2");
        }

        [Fact]
        public void ProcessBUN_Hemosheet_Should_Process_WithTimeZone_Correctly()
        {
            var sub = _fixture.Create<SystemBoundLabProcessor>();
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            var hemosheet = new HemodialysisRecord // on-going
            {
                PatientId = patientId,
                CompletedTime = null,
                Created = DateTime.UtcNow
            };
            // should not calculate anything because different day

            Action action = () => sub.ProcessBUN(hemosheet);
            action.Should().NotThrow("Should process correctly");

            labRepo.Verify(x => x.Update(Moq.It.IsAny<LabExam>()), Moq.Times.Never);
            labRepo.Verify(x => x.Insert(Moq.It.IsAny<LabExam>()), Moq.Times.Never);

            // case should not calculate timezone edge
            hemosheet.Created = TimeZoneInfo.ConvertTimeToUtc(tzNow.Date.AddHours(2), tz); // set time to 2 am today, without timezone logic, this will be treated as same day as test data
            hemosheet.CompletedTime = hemosheet.Created.Value.AddHours(4); // add for realness
            hemosheet.DialysisPrescription = new DialysisPrescription { Duration = TimeSpan.FromHours(4) };
            hemosheet.Dehydration.PostTotalWeight = 50;
            action.Should().NotThrow("Should process correctly");
            labRepo.Verify(x => x.Update(Moq.It.IsAny<LabExam>()), Moq.Times.Never);
            labRepo.Verify(x => x.Insert(Moq.It.IsAny<LabExam>()), Moq.Times.Never);

            // case should calculate timezone edge
            hemosheet.Created = TimeZoneInfo.ConvertTimeToUtc(tzNow.Date.AddDays(-1).AddHours(2), tz); // set time to 2 am yesterday, without timezone logic, this will be different day than test data
            action.Should().NotThrow("Should process correctly");
            labRepo.Verify(x => x.Insert(Moq.It.IsAny<LabExam>()), Moq.Times.AtLeastOnce, "Should calculate kt/v");
        }

        [Fact]
        public void ProcessBUN_Hemosheet_NotReady_Should_NotCalculated()
        {
            var sub = _fixture.Create<SystemBoundLabProcessor>();
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

            // case hemosheet not ready
            var hemosheet = new HemodialysisRecord // on-going
            {
                PatientId = patientId,
                CompletedTime = null,
                Created = TimeZoneInfo.ConvertTimeToUtc(tzNow.AddDays(-1).Date.AddHours(10), tz)
            };

            Action action = () => sub.ProcessBUN(hemosheet);
            action.Should().NotThrow("Should process correctly");
            labRepo.Verify(x => x.Update(Moq.It.IsAny<LabExam>()), Moq.Times.Never);
            labRepo.Verify(x => x.Insert(Moq.It.IsAny<LabExam>()), Moq.Times.Never);
        }
    }
}
