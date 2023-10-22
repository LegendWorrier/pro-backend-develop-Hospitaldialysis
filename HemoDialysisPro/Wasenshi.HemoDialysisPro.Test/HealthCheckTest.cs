using System.Net.Http;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Test.Fixture;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Test
{
    public class HealthCheckTest : IClassFixture<EnvironmentFixture>
    {
        private readonly HttpClient _client;

        public HealthCheckTest(EnvironmentFixture env)
        {
            _client = env.TestClient;
        }

        [Fact]
        public async Task Get_HealthCheckSuccess()
        {
            var response = await _client.GetAsync("/health");
            var responseText = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
            Assert.Equal("Healthy", responseText);
        }

    }
}
