using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Wasenshi.HemoDialysisPro.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class MasterDataControllerTest : ControllerTestBase, IClassFixture<EnvironmentFixture>
    {
        public MasterDataControllerTest(EnvironmentFixture env, ITestOutputHelper output) : base(env, output)
        {
        }

        [Theory]
        [InlineData("Unit")]
        [InlineData("Medicine")]
        [InlineData("Status")]
        [InlineData("DeathCause")]
        public async Task Get_List_Anonymous_Should_Success(string data)
        {
            _client.WithNoToken();
            var response = await _client.GetAsync($"/api/MasterData/{data}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resTxt = await response.Content.ReadAsStringAsync();
            JToken result = JToken.Parse(resTxt);
            result.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Post_PowerAdmin_Should_BeAbleTo_AddUnit()
        {
            var request = new UnitViewModel { Name = _fixture.Create<string>() };

            _client.WithPowerAdminToken();
            var response = await _client.PostAsync($"/api/MasterData/Unit", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Post_AddUnit_Should_Allow_Only_PowerAdmin_Or_Unit_permission()
        {
            var request = new UnitViewModel { Name = _fixture.Create<string>() };

            _client.WithAdminToken();
            var response = await _client.PostAsync($"/api/MasterData/Unit", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_EditUnit_Should_Allow_Only_PowerAdmin()
        {
            var request = new UnitViewModel { Name = _fixture.Create<string>() };

            _client.WithAdminToken();
            var response = await _client.PostAsync($"/api/MasterData/Unit/-1", GetJsonContent(request));
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_DeleteUnit_Should_Allow_Only_PowerAdmin()
        {
            _client.WithAdminToken();
            var response = await _client.DeleteAsync($"/api/MasterData/Unit/-1");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
