using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class AuthenticationTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public AuthenticationTest(EnvironmentFixture environmentFixture, ITestOutputHelper test) : base(environmentFixture, test)
        {
            _client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        }

        [Fact]
        public async Task Post_WhenLoginWithBuitinAdmin_ShouldSuccess_AndReturnToken()
        {
            var request = new LoginViewModel
            {
                Username = DataSeed.AdminUsername,
                Password = DataSeed.AdminPassword
            };

            var response = await _client.PostAsync("api/Authentication", GetJsonContent(request)).OutputResponse(output);
            var responseText = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var token = JToken.Parse(responseText);
            token["access_token"].Value<string>().Should().NotBeNullOrEmpty();
            token["expires_in"].Value<double>().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Get_When_IsNotAdmin_Should_AllowRequest()
        {
            _client.WithBasicUserToken();
            var response = await _client.GetAsync("api/Users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _client.WithHeadNurseToken();
            response = await _client.GetAsync("api/Users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _client.WithDoctorToken();
            response = await _client.GetAsync("api/Users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_When_IsAdmin_ShouldReturnListOfUsers()
        {
            _client.WithAdminToken();

            var response = await _client.GetAsync("api/Users").OutputResponse(output);
            var responseText = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var jtoken = JToken.Parse(responseText);
            Assert.True(jtoken.Type == JTokenType.Array);
        }

        [Theory]
        [InlineData("/api/Users")]
        [InlineData("/api/Users/{0}")]
        [InlineData("/api/Users/{0}/edit")]
        [InlineData("/api/Users/{0}/changerole")]
        [InlineData("/api/Authentication/register")]
        public async Task Post_EndpointsReturnFailToAnonymousUserForRestrictedUrls(string url)
        {
            _client.WithNoToken();
            var response = await _client.PostAsync(url, new StringContent(""));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Post_CreateUser_WithoutUnit_Should_Fail()
        {
            // even for powerAdmin, this should fail
            _client.WithPowerAdminToken();

            RegisterViewModel request = new RegisterViewModel
            {
                UserName = _fixture.Create<string>(),
                Password = "Test1234",
                Role = Roles.Doctor
            };

            var response = await _client.PostAsync("/api/Authentication/register", GetJsonContent(request)).OutputResponse(output);
            response.StatusCode.Should()
                .NotBe(HttpStatusCode.Created)
                .And.NotBe(HttpStatusCode.OK)
                .And.NotBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Post_CreateUser_With_Wrong_Unit_Should_Fail()
        {
            // mock user to be in unit id -2
            _client.WithAdminToken(claim: new { unit = -2 });

            RegisterViewModel request = new RegisterViewModel
            {
                UserName = _fixture.Create<string>(),
                Password = "Test1234",
                Role = Roles.Doctor,
                Units = new[] { -1 } //wrong unit
            };

            var response = await _client.PostAsync("/api/Authentication/register", GetJsonContent(request)).OutputResponse(output, "register");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            // List of units with excessed unit

            request.Units = new[] { -2, -1 };

            response = await _client.PostAsync("/api/Authentication/register", GetJsonContent(request)).OutputResponse(output, "register 2");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
