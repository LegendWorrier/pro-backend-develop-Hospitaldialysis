using DotNet.Testcontainers.Containers;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Test.Fixture.Mocks;
using Wasenshi.HemoDialysisPro.Web.Api;
using Xunit;

namespace Wasenshi.HemoDialysisPro.Test.Fixture
{
    public class EnvironmentFixture : IAsyncLifetime
    {
        private bool _dontCreateDocker;
        private IConfiguration config;
        private static PostgreSqlContainer postgres;
        private static object dbLocker = new object();

        public HttpClient TestClient { get; private set; }

        public Mock<IRedisClient> RedisClient { get; } = new Mock<IRedisClient>();
        public Mock<IMessageQueueClient> Message { get; } = new Mock<IMessageQueueClient>();
        public Mock<IBackgroundJobClient> BackgroundJob { get; } = new Mock<IBackgroundJobClient>();
        public Mock<IRecurringJobManager> RecurringJob { get; } = new Mock<IRecurringJobManager>();

        public EnvironmentFixture()
        {
        }

        public async Task DisposeAsync()
        {
            TestClient?.Dispose();
            if (_dontCreateDocker)
            {
                return;
            }
        }

        public async Task InitializeAsync()
        {
            config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            RedisClient.Setup(x => x.As<UnitShift>().GetById(It.IsAny<int>())).Returns((UnitShift)null);
            RedisClient.Setup(x => x.As<UnitShift>().GetAll()).Returns(new List<UnitShift>());
            RedisClient.Setup(x => x.Hashes[It.IsAny<string>()]).Returns<string>((s) => new RedisHashMock(s));

            Environment.SetEnvironmentVariable("maxUnits", "100");

            if (config.GetSection("CIRCLECI").Get<bool>())
            {
                Console.WriteLine("Detect CircleCi Env");
                _dontCreateDocker = true;

                // Uncomment this if circleci has problem with timezone id
                //Console.WriteLine("DEBUG: check system timezone id...");
                //foreach (var item in TimeZoneInfo.GetSystemTimeZones())
                //{
                //    Console.WriteLine($"{item.DisplayName}: {item.Id}");
                //}

                TestClient = CreateTestClient(true);
                Console.WriteLine("Create test client completed.");
            }
            else
            {
                // API Test host setup
                var postgres = CreateTestEnvironmentContainers();

                if (postgres.State != TestcontainersStates.Running)
                {
                    await postgres.StartAsync();
                }

                //_ = Wait.ForUnixContainer().UntilPortIsAvailable(postgres.GetMappedPublicPort(5432));

                TestClient = CreateTestClient(postgres);
                await OnInitialized(postgres);
            }
        }

        protected virtual Task OnInitialized(PostgreSqlContainer postgres) => Task.CompletedTask;

        private PostgreSqlContainer CreateTestEnvironmentContainers()
        {
            lock (dbLocker)
            {
                if (postgres == null)
                {
                    var builder = new PostgreSqlBuilder()
                    .WithDatabase("db")
                    .WithUsername("postgres")
                    .WithPassword("not-important")
                    .WithName("UnitTest");
                    postgres = builder.Build();
                }

                return postgres;
            }
        }


        private HttpClient CreateTestClient(PostgreSqlContainer postgres, Action<IWebHostBuilder> overrideSetting = null)
        {
            var connstr = postgres.GetConnectionString() + $";Database={Guid.NewGuid()}";
            return CreateTestClientCore(connstr, false, overrideSetting);
        }

        private HttpClient CreateTestClient(bool circleCi = false, Action<IWebHostBuilder> overrideSetting = null)
        {
            var connstr = $"host=Postgres;Port=5432;User Id=postgres;Password=admin1234;Database=HemoDialysis{Guid.NewGuid()}";
            return CreateTestClientCore(connstr, circleCi, overrideSetting);
        }

        private HttpClient CreateTestClientCore(string connstr, bool circleCi = false, Action<IWebHostBuilder> overrideSetting = null)
        {
            WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();
            return factory.WithWebHostBuilder(x =>
            {
                x.ConfigureAppConfiguration((context, config) =>
                config
                    .AddEnvironmentVariables()
                    .AddInMemoryCollection(new[]
                    {
                        //KeyValuePair.Create("DOCKER_API_VERSION", "1.25"),
                        KeyValuePair.Create("TESTING", "true"),
                        KeyValuePair.Create("ConnectionStrings:HemodialysisConnection", connstr),
                        KeyValuePair.Create("TIMEZONE", circleCi ? "TH" : "SE Asia Standard Time"),
                        KeyValuePair.Create("CULTURE", "th"),
                    }));
                x.ConfigureServices((IServiceCollection services) =>
                {
                    services
                        .ConfigureMockJwt()
                        .AddScoped<IBackgroundJobClient>(x => BackgroundJob.Object)
                        .AddScoped<IRecurringJobManager>(x => RecurringJob.Object)
                        .AddScoped<IRedisClient>(c => RedisClient.Object)
                        .AddScoped<IMessageQueueClient>(c => Message.Object);
                });
                if (overrideSetting != null)
                {
                    overrideSetting(x);
                }
            }).CreateClient();
        }


        public HttpClient OverrideTestClientSetting(Action<IWebHostBuilder> configHost)
        {
            if (config.GetSection("CIRCLECI").Get<bool>())
            {
                TestClient = CreateTestClient(true, configHost);
            }
            else
            {
                TestClient = CreateTestClient(postgres, configHost);
            }

            return TestClient;
        }
    }
}
