using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class UsersControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public UsersControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
        }

        [Fact]
        public async Task Post_RegularUserShould_Not_BeAbleTo_ChangeSelfRole()
        {
            string[] roles = new[] { Roles.Doctor };
            var content = GetJsonContent(roles);

            var requestId = await CreateUser();

            _client.WithToken(requestId);
            var response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            requestId = await CreateUser(Roles.HeadNurse);

            _client.WithHeadNurseToken(requestId);
            response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            requestId = await CreateUser(Roles.Doctor);

            _client.WithDoctorToken(requestId);
            response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_RegularUserShould_Not_BeAbleTo_ChangeOtherUserRole()
        {
            string[] roles = new[] { Roles.Doctor };
            var content = GetJsonContent(roles);
            //Target is Nurse user
            var requestId = await CreateUser();

            _client.WithBasicUserToken();
            var response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithHeadNurseToken();
            response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithDoctorToken();
            response = await _client.PostAsync($"/api/Users/{requestId}/changerole", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_RegularUserShould_Not_BeAbleTo_RegisterNewUser()
        {
            RegisterViewModel request = new RegisterViewModel { UserName = "test", Password = "TestNaja1234", Role = Roles.Nurse, Units = new[] { -1 } };
            var content = GetJsonContent(request);

            _client.WithBasicUserToken();
            var response = await _client.PostAsync("/api/Authentication/register", content).OutputResponse(output, "register");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithHeadNurseToken();
            response = await _client.PostAsync("/api/Authentication/register", content).OutputResponse(output, "register");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithDoctorToken();
            response = await _client.PostAsync("/api/Authentication/register", content).OutputResponse(output, "register");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_RegularUserShould_Not_BeAbleTo_EditOtherUsers()
        {
            var tokenId = await CreateUser();
            var requestId = await CreateUser();

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                Password = "NewPass123"
            };

            _client.WithToken(tokenId);
            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_RegularUserShould_BeAbleTo_EditSelf()
        {
            var tokenId = await CreateUser();
            var requestId = tokenId;

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                //Password = "NewPass123" except password, this need to be change via specific API
            };

            _client.WithToken(tokenId);
            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_RegularUserShould_Not_BeAbleTo_EditUsername()
        {
            var tokenId = await CreateUser();
            var requestId = tokenId;

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                Password = "NewPass123",
                //User should not be able to edit username
                UserName = $"NewUsername-{Guid.NewGuid()}"
            };

            _client.WithToken(Guid.Parse(tokenId));
            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_AdminShould_BeAbleTo_EditAnyUser()
        {
            var requestId = await CreateUser();

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                Password = "NewPass123",
                //Admin should also be able to edit username
                UserName = $"NewUsername-{Guid.NewGuid()}"
            };

            _client.WithAdminToken(claim: new { Permission = Permissions.USER });
            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_AdminShould_BeAbleTo_ChangeAnyUserRole()
        {
            var requestId = await CreateUser();

            string[] roles = new[] { Roles.Doctor };

            _client.WithAdminToken(claim: new { Permission = Permissions.USER });
            var response = await _client.PostAsync($"/api/Users/{requestId}/changerole", GetJsonContent(roles));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_AdminShould_Not_BeAbleTo_EditPowerAdminUser()
        {
            var poweradmin = await GetRootAdmin();
            var requestId = poweradmin.Id;

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                Password = "NewPass123",
                UserName = $"NewUsername-{Guid.NewGuid()}"
            };

            _client.WithAdminToken();
            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_AdminShould_Not_BeAbleTo_ChangePowerAdminRole()
        {
            var poweradmin = await GetRootAdmin();
            var requestId = poweradmin.Id;

            string[] roles = new[] { Roles.Doctor };

            _client.WithAdminToken();
            var response = await _client.PostAsync($"/api/Users/{requestId}/changerole", GetJsonContent(roles)).OutputResponse(output, "Change role");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_AdminShould_Not_BeAbleTo_Change_Themself_To_PowerAdminRole()
        {
            var requestId = await CreateAdmin();

            string[] roles = new[] { Roles.PowerAdmin, Roles.HeadNurse };

            _client.WithAdminToken(requestId);
            var response = await _client.PostAsync($"/api/Users/{requestId}/changerole", GetJsonContent(roles)).OutputResponse(output, "Change role");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_PowerAdminShould_Not_BeAbleTo_DeleteSelf()
        {
            var poweradmin = await GetRootAdmin();
            var requestId = poweradmin.Id;

            _client.WithPowerAdminToken(requestId);
            var response = await _client.DeleteAsync($"/api/Users/{requestId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_RegularUserShould_Not_BeAbleTo_DeleteAnyUser()
        {
            var requestId = await CreateUser();

            _client.WithBasicUserToken();
            var response = await _client.DeleteAsync($"/api/Users/{requestId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithHeadNurseToken();
            response = await _client.DeleteAsync($"/api/Users/{requestId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            _client.WithDoctorToken();
            response = await _client.DeleteAsync($"/api/Users/{requestId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_AdminShould_BeAbleTo_DeleteAnyUser()
        {
            var requestId = await CreateUser();

            _client.WithAdminToken(claim: new { Permission = Permissions.USER });
            var response = await _client.DeleteAsync($"/api/Users/{requestId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await GetAllUsers()).Should().NotContain(x => x.User.Id.ToString() == requestId);
        }

        [Fact]
        public async Task Delete_AdminShould_Not_BeAbleTo_DeletePowerAdminUser()
        {
            var poweradmin = await GetRootAdmin();
            var requestId = poweradmin.Id;

            _client.WithAdminToken();
            var response = await _client.DeleteAsync($"/api/Users/{requestId}").OutputResponse(output, "delete power user");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_AdminShould_Not_BeAbleTo_DeleteThemself()
        {
            // root admin trying to delete himself
            var poweradmin = await GetRootAdmin();

            _client.WithPowerAdminToken(poweradmin.Id);
            var response = await _client.DeleteAsync($"/api/Users/{poweradmin.Id}").OutputResponse(output, "delete self");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            // admin trying to delete himself
            var adminId = await CreateAdmin();
            _client.WithToken(Guid.Parse(adminId));
            response = await _client.DeleteAsync($"/api/Users/{adminId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_EditUser_WithWrongUnit_Should_Fail()
        {
            // create
            var id = await CreateUser();

            // edit
            var user = new EditUserViewModel { Units = new[] { -1, -2 } };

            _client.WithAdminToken();
            var response = await _client.PostAsync($"/api/Users/{id}/edit", GetJsonContent(user)).OutputResponse(output, "Edit");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);


            user.Units = new[] { -1 };

            _client.WithAdminToken(claim: new { unit = -2 });
            response = await _client.PostAsync($"/api/Users/{id}/edit", GetJsonContent(user)).OutputResponse(output, "Edit Wrong");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_UserList_Should_Filter_Unit_Correctly()
        {
            // check original
            _client.WithPowerAdminToken();
            var response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            var originalPowerAdminCount = token.Count();

            var units = await CreateUnits(); //deafult 3

            _client.WithAdminToken(claim: new { unit = units });
            response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            var originalCount = token.Count();

            _client.WithAdminToken(claim: new { unit = units.First() });
            response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            var originalUnit1Count = token.Count();

            // create
            await CreateUsersForUnits(Roles.Nurse, units.ToArray());

            // check all
            _client.WithAdminToken(claim: new { unit = units }); //deafult 3
            response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Count().Should().Be(originalCount + 3);

            // check powerAdmin must see all
            _client.WithPowerAdminToken();
            response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Count().Should().Be(originalPowerAdminCount + 3);

            // check limit unit
            _client.WithAdminToken(claim: new { unit = units.First() });
            response = await _client.GetAsync($"/api/Users");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Count().Should().Be(originalUnit1Count + 1); // created and rootadmin
        }

        [Fact]
        public async Task Get_DoctorList_Should_Filter_Unit_Correctly()
        {
            _client.WithPowerAdminToken();
            var response = await _client.GetAsync($"/api/Users/doctors");
            response.EnsureSuccessStatusCode();
            var responseTxt = await response.Content.ReadAsStringAsync();
            var token = JToken.Parse(responseTxt);
            var originalCount = token.Count();

            // create
            var units = await CreateUnits();
            await CreateUsersForUnits(units.ToArray()); //create doctor
            await CreateUsersForUnits(Roles.Nurse, units.ToArray());

            // check all
            _client.WithAdminToken(claim: new { unit = new[] { units[0], units[1] } });
            // no unit should filter only units the user is in
            response = await _client.GetAsync($"/api/Users/doctors").OutputResponse(output, "get doctor list");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Count().Should().Be(2);
            // check each unit
            JToken rootadmin;
            foreach (var unitId in units)
            {
                _client.WithAdminToken(claim: new { unit = unitId });
                response = await _client.GetAsync($"/api/Users/doctors?unitId={unitId}").OutputResponse(output, $"get doctor list for unit: {unitId}");
                response.EnsureSuccessStatusCode();
                responseTxt = await response.Content.ReadAsStringAsync();
                token = JToken.Parse(responseTxt);
                token.Count().Should().Be(1);
                //check must not contain rootadmin in the list
                rootadmin = token.FirstOrDefault(x => x["userName"].ToString() == "rootadmin");
                rootadmin.Should().BeNull("it must not contain rootadmin in the list");
            }

            //get wrong unit should fail
            _client.WithAdminToken(claim: new { unit = units.First() });
            response = await _client.GetAsync($"/api/Users/doctors?unitId={units.Last()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            //check powerAdmin must be able to see all
            _client.WithPowerAdminToken();
            response = await _client.GetAsync($"/api/Users/doctors");
            response.EnsureSuccessStatusCode();
            responseTxt = await response.Content.ReadAsStringAsync();
            token = JToken.Parse(responseTxt);
            token.Count().Should().Be(originalCount + 3);
            //check must not contain rootadmin in the list
            rootadmin = token.FirstOrDefault(x => x["userName"].ToString() == "rootadmin");
            rootadmin.Should().BeNull("it must not contain rootadmin in the list");
        }

        [Fact]
        public async Task Post_EditUser_Units_WithPowerAdmin_Should_Success()
        {
            // create
            var id = await CreateUser();
            var unitId = (await CreateUnits(1)).First();

            // check
            var response = await _client.GetAsync($"/api/Users/{id}");
            var original = await DeserializeAsync<UserResultResponse>(response);

            // edit
            var user = new EditUserViewModel { Units = new[] { -1, unitId } };

            _client.WithPowerAdminToken();
            response = await _client.PostAsync($"/api/Users/{id}/edit", GetJsonContent(user)).OutputResponse(output, "Edit");
            response.EnsureSuccessStatusCode();

            // check
            response = await _client.GetAsync($"/api/Users/{id}");
            var result = await DeserializeAsync<UserResultResponse>(response);

            result.Should().BeEquivalentTo(original, options => options
                .Using<ICollection<UserUnit>>(ctx => ctx.Subject.Select(x => x.UnitId).Should().BeEquivalentTo(user.Units))
                .WhenTypeIs<ICollection<UserUnit>>()
                .Excluding(x => x.User.NormalizedEmail)
                .Excluding(x => x.User.ConcurrencyStamp)
            );
        }

        [Fact]
        public async Task Post_EditUser_Basic_Information_ShouldNot_Affect_Units()
        {
            var tokenId = await CreateUser();
            var requestId = tokenId;

            _client.WithToken(tokenId);
            var response = await _client.GetAsync($"/api/Users/{requestId}");
            var original = await DeserializeAsync<UserResultResponse>(response);

            EditUserViewModel request = new EditUserViewModel
            {
                FirstName = "test",
                LastName = "Change",
                Email = "New Email",
                Units = new[] { -1 }
            };

            response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request)).OutputResponse(output, "Edit");
            response.EnsureSuccessStatusCode();

            //Check
            response = await _client.GetAsync($"/api/Users/{requestId}");
            var result = await DeserializeAsync<UserResultResponse>(response);

            result.User.Units.Select(x => x.UnitId).Should().BeEquivalentTo(request.Units);
            result.Should().BeEquivalentTo(original, options => options
                .Excluding(x => x.User.FirstName)
                .Excluding(x => x.User.LastName)
                .Excluding(x => x.User.Email)
                .Excluding(x => x.User.NormalizedEmail)
                .Excluding(x => x.User.ConcurrencyStamp)
            );
            result.User.FirstName.Should().Be(request.FirstName);
            result.User.LastName.Should().Be(request.LastName);
            result.User.Email.Should().Be(request.Email);
        }

        [Fact]
        public async Task Post_User_Should_Not_BeAbleTo_Changepassword_via_Edit_API()
        {
            var tokenId = await CreateUser();
            var requestId = tokenId;

            _client.WithToken(tokenId);

            EditUserViewModel request = new EditUserViewModel
            {
                Password = "NewPass123"
            };

            var response = await _client.PostAsync($"/api/Users/{requestId}/edit", GetJsonContent(request));
            response.StatusCode.Should().Be((HttpStatusCode)400);
        }

        [Fact]
        public async Task Post_User_Should_BeAbleTo_Changepassword_via_ChangePassword_API()
        {
            var userId = await CreateUser();
            var requestId = userId;

            _client.WithPowerAdminToken();
            var response = await _client.GetAsync($"/api/Users/{requestId}?showPwdHash=true");
            var original = await DeserializeAsync<UserResultResponse>(response);
            var originalPassHash = original.User.PasswordHash;

            EditPasswordViewModel request = new EditPasswordViewModel
            {
                Id = new Guid(userId),
                OldPassword = UserPassword,
                NewPassword = "NewPassNaja55"
            };

            _client.WithToken(userId);
            response = await _client.PostAsync($"/api/Users/changepassword", GetJsonContent(request)).OutputResponse(output, "Change password");
            response.EnsureSuccessStatusCode();

            // check
            _client.WithPowerAdminToken();
            response = await _client.GetAsync($"/api/Users/{requestId}?showPwdHash=true");
            var result = await DeserializeAsync<UserResultResponse>(response);
            result.User.PasswordHash.Should().NotBeNullOrEmpty().And.NotBe(originalPassHash);
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

        private Task<List<string>> CreateUsersForUnits(params int[] unitIds)
        {
            return CreateUsersForUnits(null, unitIds);
        }

        private async Task<List<string>> CreateUsersForUnits(string roles = null, params int[] unitIds)
        {
            List<string> ids = new List<string>();
            if (!unitIds?.Any() ?? false)
            {
                unitIds = new[] { -1, 1, 2 };
            }

            foreach (var unitId in unitIds)
            {
                var userId = await CreateUser(new[] { unitId }, roles ?? Roles.Doctor);
                ids.Add(userId);
            }

            return ids;
        }
    }
}
