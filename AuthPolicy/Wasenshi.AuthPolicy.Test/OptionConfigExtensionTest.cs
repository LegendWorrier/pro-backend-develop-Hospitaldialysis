using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Options;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class OptionConfigExtensionTest
    {
        readonly IFixture fixture;
        public OptionConfigExtensionTest()
        {
            fixture = new Fixture()
                .Customize(new AutoMoqCustomization { ConfigureMembers = true });
        }

        [Fact]
        public async Task RegisterDefaultUserConfig_Should_Work_Correctly()
        {
            var authOption = new AuthPolicyOptions();
            // Test register default user
            authOption.RegisterDefaultUserConfig();

            var userStore = new Mock<IUserStore<IdentityUser>>();
            userStore.Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new IdentityUser
                {
                    Id = "test",
                    UserName = "test"
                }));
            var roleStore = userStore.As<IUserRoleStore<IdentityUser>>();
            roleStore.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    "adminNaja"
                });

            var identityOption = new Mock<IOptions<IdentityOptions>>();
            identityOption.Setup(x => x.Value).Returns(new IdentityOptions
            {
                ClaimsIdentity = new ClaimsIdentityOptions()
            });
            var authOptionMonitor = new Mock<IOptionsMonitor<AuthPolicyOptions>>();
            authOptionMonitor.Setup(x => x.CurrentValue).Returns(authOption);

            var serviceCollection = new ServiceCollection();
            serviceCollection.TryAddTransient((_) => fixture.Create<ILogger<UserManager<IdentityUser>>>());
            serviceCollection.TryAddTransient((_) => fixture.Create<IdentityErrorDescriber>());
            serviceCollection.TryAddTransient((_) => fixture.Create<ILookupNormalizer>());
            serviceCollection.TryAddTransient((_) => fixture.Create<IPasswordHasher<IdentityUser>>());
            serviceCollection.TryAddTransient((_) => identityOption.Object);
            serviceCollection.TryAddTransient((_) => authOptionMonitor.Object);
            serviceCollection.TryAddTransient((_) => userStore.Object);
            serviceCollection.TryAddTransient<UserManager<IdentityUser>>();
            var service = serviceCollection.BuildServiceProvider();

            var userConfig = authOption.GetUserConfig<DefaultUserConfig, string>(service);
            userConfig.Should().NotBeNull();

            var claimMock = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testId"),
                new Claim(ClaimTypes.Name, "test"),
                new Claim(ClaimTypes.Role, "admin")
            }));
            var id = userConfig.GetUserId(claimMock);
            id.Should().Be("testId");

            var roles = userConfig.GetRolesFromClaims(claimMock);
            roles.Should().Contain("admin");

            roles = await userConfig.GetUserRolesAsync("test");
            roles.Should().Contain("adminNaja");
        }

        [Fact]
        public void RegisterDuplicatedUserConfig_Should_Throw_Error()
        {
            var authOption = new AuthPolicyOptions();
            // Test register default user
            authOption.RegisterDefaultUserConfig();

            Action act = () => authOption.RegisterDefaultUserConfig();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetUserConfig_WithoutRegistering_Should_Throw_Error()
        {
            var authOption = new AuthPolicyOptions();
            var service = fixture.Create<IServiceProvider>();

            Action act = () => authOption.GetUserConfig<DefaultUserConfig, string>(service);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddHandler_Should_Success()
        {
            var authOption = new AuthPolicyOptions();

            authOption.AddHandler(Assembly.GetExecutingAssembly());

            authOption.AddHandler(typeof(AuthHandlers.ResourcePermissionHandlerDefault));

            Assert.True(true);
        }

        [Fact]
        public void AddPolicy_Should_Success()
        {
            var authOption = new AuthPolicyOptions();

            authOption.AddPolicy("Test", fixture.Create<IAuthPolicy<int>>(), (c) => {});

            Assert.True(true);
        }
    }
}
