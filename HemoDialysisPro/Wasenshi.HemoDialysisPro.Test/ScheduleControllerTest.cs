using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class ScheduleControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        private UpdateUnitSectionViewModel model;

        private TimeZoneInfo tz;

        public ScheduleControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
            model = new UpdateUnitSectionViewModel
            {
                SectionList = new[]
                {
                    new ScheduleSectionViewModel
                    {
                        StartTime = 4 *60 // 4 AM
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 8 *60 // 8 AM
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 12 *60 // 12 PM
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 16 *60 // 4 PM
                    }
                }
            };

            // test environment fixture use +7 timezone
            tz = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.BaseUtcOffset == TimeSpan.FromHours(7));
        }

        [Fact]
        public async Task Post_ScheduleSectionCreation_Should_Work_CorrectlyAsync()
        {
            var unit = await CreateUnits(1);
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}/sections").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<GetSectionsViewModel>(response);
            result.Sections.Should().HaveCount(4);
        }

        [Fact]
        public async Task Post_ScheduleSectionCreation_Should_Block_Invalid_Data()
        {
            UpdateUnitSectionViewModel model = new UpdateUnitSectionViewModel
            {
                SectionList = new[]
                {
                    new ScheduleSectionViewModel
                    {
                        StartTime = 4 *60
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 5 *60
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 10 *60
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = 12 *60
                    }
                }
            };

            var unit = await CreateUnits(1);
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.StatusCode.Should().NotBe((System.Net.HttpStatusCode)200);
        }

        [Fact]
        public async Task Post_ScheduleSectionCreation_Should_Block_Invalid_Unit()
        {
            // User with no permission for unit -1
            _client.WithAdminToken(null, new { unit = -2 });
            var response = await _client.PostAsync($"/api/Schedule/{-1}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Post_ScheduleSectionCreation_Should_Block_Regular_Nurse_And_DoctorAsync()
        {
            // Create user for unit
            var userId = await CreateUser();
            // Create unit
            int unitId = (await CreateUnits(1)).First();

            // create schedule sections with regular Nurse
            _client.WithToken(userId, null, new { unit = unitId });
            var response = await _client.PostAsync($"/api/Schedule/{unitId}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)403);

            // create schedule sections with Doctor
            _client.WithToken(userId, new[] { Roles.Doctor }, new { unit = unitId });
            response = await _client.PostAsync($"/api/Schedule/{unitId}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.StatusCode.Should().Be((System.Net.HttpStatusCode)403);
        }

        [Fact]
        public async Task Post_ScheduleSectionCreation_Should_Allow_UnitHead_RegardlessOfRoleAsync()
        {
            // Create head user for unit
            var userId = await CreateUser();
            // Create unit with head nurse
            var request = new UnitViewModel { Name = _fixture.Create<string>(), HeadNurse = new Guid(userId) };
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/masterdata/unit", GetJsonContent(request));
            response.IsSuccessStatusCode.Should().BeTrue();
            int unitId = int.Parse(response.Headers.Location.OriginalString);

            // create schedule sections with userId
            _client.WithToken(userId, null, new { unit = unitId });
            response = await _client.PostAsync($"/api/Schedule/{unitId}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unitId}/sections").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<GetSectionsViewModel>(response);
            result.Sections.Should().HaveCount(4);
        }

        [Fact]
        public async Task Post_SlotPatient_Should_Work_Correctly()
        {
            var unit = await CreateUnits(1);
            // setup sections
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
            var sections = await DeserializeAsync<IEnumerable<ScheduleSectionViewModel>>(response);

            // setup patients
            var patientList = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var patient = await CreatePatientAsync(null, unit[0]);
                patientList.Add(patient.Id);
            }

            // put into slots
            SectionSlotPatientViewModel slot = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            SectionSlotPatientViewModel slot2 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[1],
                SectionId = sections.Skip(1).First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot2)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<ScheduleResultViewModel>(response);
            result.UnitId.Should().Be(unit[0]);
            result.Sections.Should().HaveCount(4);
            result.Sections.First().Slots.Should().HaveCount(7)
                .And.Subject.Skip(1).First().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot.PatientId);
            result.Sections.First().Slots.Skip(1).First().SectionStartTime.Should().Be(240);

            result.Sections.Skip(1).First().Slots.Last().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);
        }

        [Fact]
        public async Task Post_SlotPatient_Should_Allow_Multiple_ScheduleForPatient()
        {
            var unit = await CreateUnits(1);
            // setup sections
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
            var sections = await DeserializeAsync<IEnumerable<ScheduleSectionViewModel>>(response);

            // setup patients
            var patientList = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                var patient = await CreatePatientAsync(null, unit[0]);
                patientList.Add(patient.Id);
            }

            SectionSlotPatientViewModel slot = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();
            // multiple slots for patients
            SectionSlotPatientViewModel slot2 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.Skip(2).First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot2)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            SectionSlotPatientViewModel slot3 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[1],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot3)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<ScheduleResultViewModel>(response);

            result.Sections.First().Slots.Should().HaveCount(7)
                .And.Subject.Skip(1).First().PatientList.Should().HaveCount(2)
                .And.Subject.Select(x => x.PatientId).Should()
                    .Contain(new[] { slot.PatientId, slot3.PatientId })
                    .And.OnlyHaveUniqueItems();

            result.Sections.Skip(2).First().Slots.Last().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);

            // Move slot (change section on the same slotday) <- should not allow, should use swap API instead
            SectionSlotPatientViewModel slot4 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot4)).OutputResponse(output, "slot");
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task Post_SlotPatient_Should_Swap_Correctly()
        {
            var unit = await CreateUnits(1);
            // setup sections
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
            var sections = await DeserializeAsync<IEnumerable<ScheduleSectionViewModel>>(response);

            // setup patients
            var patientList = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                var patient = await CreatePatientAsync(null, unit[0]);
                patientList.Add(patient.Id);
            }

            SectionSlotPatientViewModel slot = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            SectionSlotPatientViewModel slot2 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[1],
                SectionId = sections.Skip(1).First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot2)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<ScheduleResultViewModel>(response);

            result.Sections.First().Slots.Should().HaveCount(7)
                .And.Subject.Skip(1).First().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot.PatientId);

            result.Sections.Skip(1).First().Slots.Last().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);

            // Swap
            var swapRequest = new SwapSlotViewModel
            {
                first = slot,
                second = slot2,
            };
            response = await _client.PostAsync($"/api/Schedule/slots/swap", GetJsonContent(swapRequest)).OutputResponse(output, "swap");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            result = await DeserializeAsync<ScheduleResultViewModel>(response);

            result.Sections.First().Slots.Should().HaveCount(7)
                .And.Subject.Skip(1).First().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);

            result.Sections.Skip(1).First().Slots.Last().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot.PatientId);
        }

        [Fact]
        public async Task Post_Reschedule_Should_Work_Correctly()
        {
            var tzDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            tzDate = tzDate.AddTicks(-tzDate.TimeOfDay.Ticks);
            var unit = await CreateUnits(1);
            // setup sections
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
            var sections = await DeserializeAsync<IEnumerable<ScheduleSectionViewModel>>(response);

            // setup patients
            var patientList = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var patient = await CreatePatientAsync(null, unit[0]);
                patientList.Add(patient.Id);
            }

            SectionSlotPatientViewModel slot = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            SectionSlotPatientViewModel slot2 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[1],
                SectionId = sections.Skip(1).First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot2)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<ScheduleResultViewModel>(response);
            result.UnitId.Should().Be(unit[0]);
            result.Sections.Should().HaveCount(4);
            result.Sections.First().Slots.Should().HaveCount(7)
                .And.Subject.Skip(1).First().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot.PatientId);
            result.Sections.First().Slots.Skip(1).First().SectionStartTime.Should().Be(240);

            result.Sections.Skip(1).First().Slots.Last().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);

            // Reschedule
            var req = new RescheduleViewModel
            {
                Date = tzDate.AddDays(1).AddMinutes(sections.Skip(1).First().StartTime)
            };
            response = await _client
                .PostAsync($"/api/Schedule/reschedule/{slot2.PatientId}/{slot2.SectionId}/{slot2.Slot}",
                GetJsonContent(req)).OutputResponse(output, "reschedule");
            response.EnsureSuccessStatusCode();

            var req2 = new RescheduleViewModel
            {
                Date = tzDate.AddDays(-1).AddMinutes(sections.Skip(2).First().StartTime)
            };
            response = await _client
                .PostAsync($"/api/Schedule/reschedule/{slot2.PatientId}/{slot2.SectionId}/{slot2.Slot}",
                GetJsonContent(req2)).OutputResponse(output, "reschedule invalid");
            response.IsSuccessStatusCode.Should().BeFalse("Should not allow past schedule");

            response = await _client
                .PostAsync($"/api/Schedule/reschedule/{patientList[2]}/{slot2.SectionId}/{slot2.Slot}",
                GetJsonContent(req)).OutputResponse(output, "reschedule invalid");
            response.IsSuccessStatusCode.Should().BeFalse("Wrong patient id should not allow");

            var req3 = new RescheduleViewModel
            { // slot 1 is Tue and first section, so original date should accord to that
                Date = tzDate.AddDays(1).AddMinutes(sections.Skip(1).First().StartTime),
                OriginalDate = tzDate.AddDays(7).AddDays(DayOfWeek.Tuesday - tzDate.DayOfWeek).AddMinutes(sections.First().StartTime)
            };
            response = await _client
                .PostAsync($"/api/Schedule/reschedule/{slot.PatientId}/{slot.SectionId}/{slot.Slot}",
                GetJsonContent(req)).OutputResponse(output, "reschedule");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            result = await DeserializeAsync<ScheduleResultViewModel>(response);
            result.Reschedules.Should().HaveCount(2);
            var check = result.Reschedules.First();
            check.Should().BeEquivalentTo(slot2, c =>
                c.Using<Guid?>(x => x.Should().NotBeNull())
                    .When(x => x.Path.ToLower().Contains("by"))
                    .Using<DateTimeOffset?>(x => x.Should().NotBeNull())
                    .When(x => x.Path.ToLower() == "created" || x.Path.ToLower() == "updated"));
            check.Date.Should().Be(req.Date);
            // check 2nd
            check = result.Reschedules.Last();
            check.Should().BeEquivalentTo(slot, c =>
                c.Using<Guid?>(x => x.Should().NotBeNull())
                    .When(x => x.Path.ToLower().Contains("by"))
                    .Using<DateTimeOffset?>(x => x.Should().NotBeNull())
                    .When(x => x.Path.ToLower() == "created" || x.Path.ToLower() == "updated"));
            check.Date.Should().Be(req3.Date);
        }

        [Fact]
        public async Task Post_Schedule_ModifiedOrDelete_ShouldNot_Break_Schedule()
        {
            var unit = await CreateUnits(1);
            // setup sections
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(model)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
            var sections = await DeserializeAsync<IEnumerable<ScheduleSectionViewModel>>(response);

            // setup patients
            var patientList = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var patient = await CreatePatientAsync(null, unit[0]);
                patientList.Add(patient.Id);
            }
            SectionSlotPatientViewModel slot = new SectionSlotPatientViewModel
            {
                PatientId = patientList[0],
                SectionId = sections.First().Id,
                Slot = SectionSlots.Tue
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            SectionSlotPatientViewModel slot2 = new SectionSlotPatientViewModel
            {
                PatientId = patientList[1],
                SectionId = sections.Skip(1).First().Id,
                Slot = SectionSlots.Sun
            };
            response = await _client.PostAsync($"/api/Schedule/{unit[0]}/slots", GetJsonContent(slot2)).OutputResponse(output, "slot");
            response.EnsureSuccessStatusCode();

            var tzDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            // Reschedule slot2
            var req = new RescheduleViewModel
            {
                Date = tzDate.AddTicks(-tzDate.TimeOfDay.Ticks).AddMinutes(sections.Skip(3).First().StartTime).AddDays(1)
            };
            output.WriteLine("DEBUG: reschedule to target date: {0}", req.Date);
            output.WriteLine(JsonConvert.SerializeObject(sections, Formatting.Indented));
            response = await _client
                .PostAsync($"/api/Schedule/reschedule/{slot2.PatientId}/{slot2.SectionId}/{slot2.Slot}",
                GetJsonContent(req)).OutputResponse(output, "reschedule");
            response.EnsureSuccessStatusCode();

            // move slot2
            SwapSlotViewModel swap = new SwapSlotViewModel
            {
                first = slot2,
                second = new SectionSlotPatientViewModel { SectionId = sections.Last().Id, Slot = SectionSlots.Mon }
            };
            response = await _client
                .PostAsync($"/api/Schedule/slots/swap",
                GetJsonContent(swap)).OutputResponse(output, "swap/move");
            response.EnsureSuccessStatusCode();

            // Check
            response = await _client.GetAsync($"/api/Schedule/{unit[0]}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();

            var result = await DeserializeAsync<ScheduleResultViewModel>(response);
            result.Reschedules.Should().HaveCount(1); // change schedule should not affected rescheduled
            result.Sections.Last().Slots.First().PatientList.Should().HaveCount(1)
                .And.Subject.First().PatientId.Should().Be(slot2.PatientId);
        }

        // ========================== Utils ================================

        private async Task<List<int>> CreateUnits(int count = 3)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var request = new UnitViewModel { Name = _fixture.Create<string>() };
                _client.WithPowerAdminToken();
                var response = await _client.PostAsync("/api/masterdata/unit", GetJsonContent(request));
                var id = int.Parse(response.Headers.Location.OriginalString);
                ids.Add(id);
            }
            return ids;
        }
    }
}
