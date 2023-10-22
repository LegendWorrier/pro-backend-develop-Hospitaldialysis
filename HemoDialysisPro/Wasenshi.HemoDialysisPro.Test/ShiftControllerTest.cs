using AutoFixture;
using FluentAssertions;
using Moq;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class ShiftControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        private TimeZoneInfo tz;

        private Mock<IRedisClient> redisClient;
        private Mock<IMessageQueueClient> message;

        public ShiftControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
            // test environment fixture use +7 timezone
            tz = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.BaseUtcOffset == TimeSpan.FromHours(7));

            redisClient = env.RedisClient;
            message = env.Message;
        }

        [Fact]
        public async Task Post_SaveShifts_Should_Work_CorrectlyAsync()
        {
            List<int> units = await CreateUnits(2);
            Guid userId = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));
            Guid userId2 = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));

            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var dateNow = DateOnly.FromDateTime(tzNow.LocalDateTime);
            // ensure time edge resilence for test (make sure all the input slots are within the same month)
            bool inverse = tzNow.Day > 26;
            ShiftsEditViewModel request = new ShiftsEditViewModel
            {
                ShiftSlots = new List<ShiftSlotViewModel>
                {
                    new ShiftSlotViewModel
                    {
                        Date = dateNow,
                        ShiftData = ShiftData.Reserved,
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(inverse ? -1 : 1),
                        ShiftData = ShiftData.OffLimit,
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(inverse ? -2 : 2),
                        ShiftData = ShiftData.Section1 | ShiftData.Section2,
                        UnitId = units[0],
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(inverse ? -1 : 1),
                        ShiftData = ShiftData.Section2 | ShiftData.Section3,
                        UnitId = units[1],
                        UserId = userId2
                    }
                }
            };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Shift", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            // with only generated units
            _client.WithAdminToken(claim: new { unit = units });
            response = await _client.GetAsync($"/api/Shift").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<ShiftResultViewModel>(response);
            result.Month.Month.Should().Be(tzNow.Month);
            result.Users.Should().HaveCount(2);

            var first = result.Users.First(x => x.UserId == userId);
            first.Should().NotBeNull();
            first.Suspended.Should().BeFalse();
            first.ShiftSlots.Should().HaveCount(3);

            var second = result.Users.First(x => x.UserId == userId2);
            second.Should().NotBeNull();
            second.Suspended.Should().BeFalse();
            second.ShiftSlots.Should().HaveCount(1);
        }

        [Fact]
        public async Task Get_ShiftsForRootadmin_Should_have_All()
        {
            List<int> units = await CreateUnits(1);
            Guid userId = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));

            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var dateNow = DateOnly.FromDateTime(tzNow.LocalDateTime);
            ShiftsEditViewModel request = new ShiftsEditViewModel
            {
                ShiftSlots = new List<ShiftSlotViewModel>
                {
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(1),
                        ShiftData = ShiftData.Section2 | ShiftData.Section3,
                        UnitId = units[0],
                        UserId = userId
                    }
                }
            };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Shift", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            // with rootadmin
            _client.WithPowerAdminToken();
            response = await _client.GetAsync($"/api/Shift").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<ShiftResultViewModel>(response);
            result.Month.Month.Should().Be(tzNow.Month);
            result.Users.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact]
        public async Task Post_SuspendUser_Should_Work_Correctly()
        {
            List<int> units = await CreateUnits(1);
            Guid userId = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));
            Guid userId2 = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));

            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var dateNow = DateOnly.FromDateTime(tzNow.LocalDateTime);
            ShiftsEditViewModel request = new ShiftsEditViewModel
            {
                ShiftSlots = Enumerable.Empty<ShiftSlotViewModel>(),
                SuspendedList = new List<UserShiftEditViewModel>
                {
                    new UserShiftEditViewModel
                    {
                        UserId = userId,
                        Suspended = true,
                        Month = dateNow,
                    }
                }
            };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Shift", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            // with only generated units
            _client.WithAdminToken(claim: new { unit = units });
            response = await _client.GetAsync($"/api/Shift").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<ShiftResultViewModel>(response);
            result.Month.Month.Should().Be(tzNow.Month);
            result.Users.Should().HaveCount(2);
            result.Users.First(x => x.UserId == userId).Should().NotBeNull();
            result.Users.First(x => x.UserId == userId).Suspended.Should().BeTrue();
        }

        [Fact]
        public async Task Get_HistoryList_Should_Work_Correctly_And_BeAbleTo_GetShifts_Specific_Month()
        {
            List<int> units = await CreateUnits(2);
            Guid userId = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));
            Guid userId2 = new Guid(await CreateUser(units.ToArray(), Roles.Nurse));

            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var dateNow = DateOnly.FromDateTime(tzNow.DateTime);
            bool invert = tzNow.Day > 25;

            ShiftsEditViewModel request = new ShiftsEditViewModel
            {
                ShiftSlots = new List<ShiftSlotViewModel>
                {
                    new ShiftSlotViewModel
                    {
                        Date = dateNow,
                        ShiftData = ShiftData.Reserved,
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(invert ? -1 : 1),
                        ShiftData = ShiftData.OffLimit,
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(invert ? -2 : 2),
                        ShiftData = ShiftData.Section1 | ShiftData.Section2,
                        UnitId = units[0],
                        UserId = userId
                    },
                    new ShiftSlotViewModel
                    {
                        Date = dateNow.AddDays(invert ? -1 : 1),
                        ShiftData = ShiftData.Section2 | ShiftData.Section3,
                        UnitId = units[1],
                        UserId = userId2
                    }
                }
            };

            // this month
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Shift", GetJsonContent(request)).OutputResponse(output, $"This month ({tzNow.Month})");
            response.EnsureSuccessStatusCode();
            // last month
            var lastMonth = tzNow.Date.AddDays(-tzNow.Day + 1).AddMonths(-1);
            response = await _client.PostAsync($"/api/Shift?month={lastMonth:yyyy/MM/dd}", GetJsonContent(request)).OutputResponse(output, $"Last month ({lastMonth.Month})");
            response.EnsureSuccessStatusCode();
            // last 2 month
            var last2month = tzNow.Date.AddDays(-tzNow.Day + 1).AddMonths(-2);
            response = await _client.PostAsync($"/api/Shift?month={last2month:yyyy/MM/dd}", GetJsonContent(request)).OutputResponse(output, $"Last 2 month ({last2month.Month})");
            response.EnsureSuccessStatusCode();

            // Check
            // with only generated units
            _client.WithAdminToken(claim: new { unit = units });
            response = await _client.GetAsync($"/api/Shift/list").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var result = await DeserializeAsync<List<DateTime>>(response);

            result.Should().HaveCount(2);
            result.Should().ContainInOrder(lastMonth, last2month);

            // Check Get last month
            response = await _client.GetAsync($"/api/Shift?month={lastMonth:yyyy/MM/dd}").OutputResponse(output, "Get");
            response.EnsureSuccessStatusCode();
            var shiftViewResult = await DeserializeAsync<ShiftResultViewModel>(response);
            shiftViewResult.Month.Month.Should().Be(lastMonth.Month);
            shiftViewResult.Users.Should().HaveCount(2);
        }

        [Fact]
        public async Task Post_AddIncharge_Should_Work_Correctly()
        {
            var unit = await CreateUnits(1);
            var userId = new Guid(await CreateUser(unit.ToArray()));

            // FE must normalize the timestamp to be zero offset timezone, or compatible format for DateOnly
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var request = new List<InchargeViewModel>
            {
                new InchargeViewModel
                {
                    Date = DateOnly.FromDateTime(tzNow.DateTime),
                    UserId = userId,
                    UnitId = unit[0]
                }
            };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Shift/incharges", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Get_IsIncharge_Should_Work_Correctly()
        {
            var unit = await CreateUnits(1);
            // setup unitshift
            redisClient.Setup(x => x.As<UnitShift>().GetById(It.IsAny<int>())).Returns(new UnitShift
            {
                Id = unit[0],
                CurrentShift = -1
            });

            var userId = new Guid(await CreateUser(unit.ToArray()));

            // Check before incharge
            _client.WithToken(userId);
            var response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output);
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false");

            // FE must normalize the timestamp to be zero offset timezone
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow), tz);
            var request = new List<InchargeViewModel>
            {
                new InchargeViewModel
                {
                    Date = DateOnly.FromDateTime(tzNow.DateTime),
                    UserId = userId,
                    UnitId = unit[0]
                }
            };

            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Shift/incharges", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check after incharge
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output);
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("true", "Should be incharge.");
        }

        [Fact]
        public async Task Get_IsIncharge_Should_ReturnFalse_ForYesterdayIncharge()
        {
            var unit = await CreateUnits(1);
            // setup unitshift
            redisClient.Setup(x => x.As<UnitShift>().GetById(It.IsAny<int>())).Returns(new UnitShift
            {
                Id = unit[0],
                CurrentShift = -1
            });
            var userId = new Guid(await CreateUser(unit.ToArray()));

            // Check before incharge
            _client.WithToken(userId);
            var response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false");

            // FE must normalize the timestamp to be zero offset timezone
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var request = new List<InchargeViewModel>
            {
                new InchargeViewModel
                {
                    Date = DateOnly.FromDateTime(tzNow.AddDays(-1).DateTime),
                    UserId = userId,
                    UnitId = unit[0]
                }
            };

            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Shift/incharges", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check after incharge
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false", "Should not be incharge.");
        }

        [Fact]
        public async Task Get_IsIncharge_Should_Work_Correctly_ForSpecific_SectionTime()
        {
            // FE must normalize the timestamp to be zero offset timezone
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);

            var unit = await CreateUnits(1);
            // with sections for current time
            // (ensure within 24 hour)
            int firstStart = tzNow.Hour - 1 < 0 ? 23 : (tzNow.Hour - 1);
            int secondStart = tzNow.Hour + 3 > 23 ? (tzNow.Hour + 3 - 24) : (tzNow.Hour + 3);
            var sectionsRequest = new UpdateUnitSectionViewModel
            {
                SectionList = new[]
                {
                    new ScheduleSectionViewModel
                    {
                        StartTime = (int)TimeSpan.FromHours(firstStart).TotalMinutes
                    },
                    new ScheduleSectionViewModel
                    {
                        StartTime = (int)TimeSpan.FromHours(secondStart).TotalMinutes
                    }
                }
            };
            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Schedule/{unit[0]}/sections/update", GetJsonContent(sectionsRequest)).OutputResponse(output, "create sections");
            response.EnsureSuccessStatusCode();
            // get sections
            response = await _client.GetAsync($"/api/schedule/{unit[0]}/sections").OutputResponse(output, "get sections");
            response.EnsureSuccessStatusCode();
            var sectionsView = await DeserializeAsync<GetSectionsViewModel>(response);
            // Mock according to sections data
            // setup unitshift
            var unitShift = new UnitShift
            {
                Id = unit[0],
                CurrentShift = 0,
                Sections = sectionsView.Sections.Select(s => new Models.ScheduleSection
                {
                    Id = s.Id,
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(s.StartTime))
                }).ToList()
            };
            redisClient.Setup(x => x.As<UnitShift>().GetById(It.IsAny<int>())).Returns(unitShift);

            var userId = new Guid(await CreateUser(new[] { unit[0] }));

            // Check before incharge
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false");


            var request = new List<InchargeViewModel>
            {
                new InchargeViewModel
                {
                    Date = DateOnly.FromDateTime(tzNow.DateTime),
                    UnitId = unit[0],
                    Sections = new List<InchargeSectionViewModel>
                    {
                        new InchargeSectionViewModel
                        {
                            SectionId = unitShift.CurrentSection.Id,
                            UserId = userId
                        }
                    }
                }
            };

            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Shift/incharges", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check after incharge

            // too late case
            unitShift.CurrentShift = 1;
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false", "Should not be incharge.");

            // correct section case
            unitShift.CurrentShift = 0;
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("true", "Should be incharge.");



            // too early
            request[0].Sections = new List<InchargeSectionViewModel>
            {
                new InchargeSectionViewModel
                {
                    SectionId = unitShift.Sections[1].Id,
                    UserId = userId
                }
            };
            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Shift/incharges", GetJsonContent(request)).OutputResponse(output);
            response.EnsureSuccessStatusCode();

            // Check
            _client.WithToken(userId);
            response = await _client.GetAsync($"/api/Shift/{unit[0]}/incharge/check").OutputResponse(output, "check");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("false", "Should not be incharge.");
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
