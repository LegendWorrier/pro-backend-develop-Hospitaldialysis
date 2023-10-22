using AutoFixture;
using DateOnlyTimeOnly.AspNet.Converters;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Converters;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Wasenshi.HemoDialysisPro.Test.Fixture
{
    public abstract class ControllerTestBase
    {
        protected readonly HttpClient _client;
        protected readonly EnvironmentFixture _env;
        protected readonly IFixture _fixture;
        protected readonly ITestOutputHelper output;

        public const string UserPassword = "TestNaja1234";

        public ControllerTestBase(EnvironmentFixture env, ITestOutputHelper output)
        {
            _client = env.TestClient;
            _env = env;
            _fixture = new AutoFixture.Fixture();
            _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTime>()));
            _fixture.Register(() => TimeOnly.FromDateTime(_fixture.Create<DateTime>()));
            this.output = output;
        }

        protected void WithRoleToken(string role)
        {
            switch (role)
            {
                case Roles.Admin:
                    _client.WithAdminToken();
                    break;
                case Roles.Doctor:
                    _client.WithDoctorToken();
                    break;
                case Roles.HeadNurse:
                    _client.WithHeadNurseToken();
                    break;
                case Roles.Nurse:
                    _client.WithBasicUserToken();
                    break;
                case Roles.PN:
                    _client.WithPNUserToken();
                    break;
            }
        }

        protected async Task<string> CreateAdmin()
        {
            return await CreateUser(Roles.Admin);
        }

        protected Task<string> CreateUser(params string[] overrideRoles)
        {
            return CreateUser(null, overrideRoles);
        }

        /// <summary>
        /// Default user is Nurse.
        /// </summary>
        /// <param name="overrideRoles"></param>
        /// <returns></returns>
        protected async Task<string> CreateUser(int[] units = null, params string[] overrideRoles)
        {
            RegisterViewModel request = new RegisterViewModel { UserName = $"test-{Guid.NewGuid()}", Password = UserPassword, Role = Roles.Nurse, isAdmin = overrideRoles?.Contains(Roles.Admin) ?? false, Units = units ?? new[] { -1 } };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync("/api/Authentication/register", GetJsonContent(request));

            response.StatusCode.Should().Be(HttpStatusCode.Created, "PowerAdmin should always be able to create new user successfully.");

            var responseTxt = await response.Content.ReadAsStringAsync();
            var id = JsonConvert.DeserializeObject<string>(responseTxt);

            if (overrideRoles.Any(s => s != Roles.Admin))
            {
                response = await _client.PostAsync($"/api/Users/{id}/changerole", GetJsonContent(overrideRoles));
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            return id;
        }

        protected async Task<IUser> GetRootAdmin()
        {
            var users = await GetAllUsers();
            return users.First(x => x.Roles.Contains(Roles.PowerAdmin)).User;
        }

        protected async Task<List<UserResultResponse>> GetAllUsers()
        {
            _client.WithPowerAdminToken();

            var response = await _client.GetAsync("api/Users");

            return await DeserializeAsync<List<UserResultResponse>>(response);
        }

        public class UserResultResponse
        {
            public User User { get; set; } // User concrete class to receive all information
            public IList<string> Roles { get; set; }
        }

        // ============ patient util ==================
        protected async Task<CreatePatientViewModel> CreatePatientAsync(string doctorId = null, int unitId = -1,
            ICollection<int> allergy = null,
            ICollection<TagViewModel> tags = null,
            DialysisInfoViewModel dialysis = null,
            EmergencyContactViewModel emergency = null)
        {
            var id = _fixture.Create<string>();
            Guid? doctorGuid = !string.IsNullOrWhiteSpace(doctorId) ? Guid.Parse(doctorId) : (Guid?)null;
            CreatePatientViewModel request = new CreatePatientViewModel { Name = _fixture.Create<string>(), BirthDate = new DateTime(1992, 11, 5), Id = id, HospitalNumber = "HN", IdentityNo = "123456789", UnitId = unitId, DoctorId = doctorGuid };
            request.Allergy = allergy;
            request.Tags = tags;
            request.DialysisInfo = dialysis;
            request.EmergencyContact = emergency;

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/Patients", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            return request;
        }

        protected StringContent GetJsonContent(object content)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateOnlyJsonConverter());
            options.Converters.Add(new TimeOnlyJsonConverter());
            var json = JsonSerializer.Serialize(content, options);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected async Task<T> DeserializeAsync<T>(HttpResponseMessage response) where T : class
        {
            T result = null;

            await response.Invoking(async x =>
            {
                var text = await response.Content.ReadAsStringAsync();
                result = Deserialize<T>(text);
            })
                .Should()
                .NotThrowAsync();
            return result;
        }
        protected T Deserialize<T>(string jsonObj) where T : class
        {
            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            option.Converters.Add(new IUserToUserConverter());
            option.Converters.Add(new UnitCollectionConverter());
            option.Converters.Add(new DateOnlyJsonConverter());
            option.Converters.Add(new TimeOnlyJsonConverter());
            return JsonSerializer.Deserialize<T>(jsonObj, option);
        }
    }

    internal static class Extension
    {
        public static async Task<HttpResponseMessage> OutputResponse(this Task<HttpResponseMessage> response, ITestOutputHelper output, string name = "")
        {
            var content = await response;

            var msg = "response: ";
            if (name != "")
            {
                msg = name + $" {msg}";
            }
            output.WriteLine(msg);
            output.WriteLine(await content.Content.ReadAsStringAsync());

            return content;
        }
    }

    internal class IUserToUserConverter : IUserConverter<User>
    {
    }

}
