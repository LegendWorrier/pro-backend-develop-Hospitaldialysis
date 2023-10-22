using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Requirements;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class PolicyConfigTest
    {
        readonly ServiceCollection _collection;
        public PolicyConfigTest()
        {
            _collection = new ServiceCollection();
            _collection.AddLogging();
            _collection.AddAuthorizationCore();
        }

        [Fact]
        public async Task WhenAddPolicy_ShouldAutomateSetupSuccessfully()
        {
            _collection.AddPolicy();

            var provider = _collection.BuildServiceProvider();
            var auth = provider.GetService<IAuthorizationService>();
            var result = await auth.AuthorizeAsync(new ClaimsPrincipal(), new TestModel(), new ResourcePermissionRequirement());

            result.Succeeded.Should().Be(true);
        }

        [Fact]
        public async Task WhenAddPolicyWithTypeMarking_ShouldAutomateSetupSuccessfully()
        {
            _collection.AddPolicy(typeof(TestOwnerHandler));

            var provider = _collection.BuildServiceProvider();
            var auth = provider.GetService<IAuthorizationService>();
            var result = await auth.AuthorizeAsync(new ClaimsPrincipal(), new TestModel(), new ResourcePermissionRequirement());

            result.Succeeded.Should().Be(true);
        }

        [Fact]
        public async Task WhenAddPolicyWithAssemblyScan_ShouldAutomateSetupSuccessfully()
        {
            _collection.AddPolicy(Assembly.GetExecutingAssembly());

            var provider = _collection.BuildServiceProvider();
            var auth = provider.GetService<IAuthorizationService>();
            var result = await auth.AuthorizeAsync(new ClaimsPrincipal(), new TestModel(), new ResourcePermissionRequirement());

            result.Succeeded.Should().Be(true);
        }
    }
}
