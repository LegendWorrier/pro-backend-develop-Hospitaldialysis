using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Requirements;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class OwnerHandlerDefaultTest
    {
        ServiceCollection _collection;
        public OwnerHandlerDefaultTest()
        {
            _collection = new ServiceCollection();
            _collection.AddLogging();
            _collection.AddAuthorizationCore();
        }

        [Fact]
        public async Task WhenNoHandler_ShouldThrow()
        {
            _collection.AddSingleton<IAuthorizationHandler, ResourcePermissionHandlerDefault>();
            var provider = _collection.BuildServiceProvider();
            var auth = provider.GetService<IAuthorizationService>();

            Func<Task> test = () => auth.AuthorizeAsync(new ClaimsPrincipal(), new TestModel(), new ResourcePermissionRequirement());
            await test.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task When_HandlerExists_ShouldDoNothing()
        {
            //ORDER IS IMPORTANT here
            _collection.AddSingleton<IAuthorizationHandler, TestOwnerHandler>();
            _collection.AddSingleton<IAuthorizationHandler, ResourcePermissionHandlerDefault>();

            var provider = _collection.BuildServiceProvider();
            var auth = provider.GetService<IAuthorizationService>();
            var result = await auth.AuthorizeAsync(new ClaimsPrincipal(), new TestModel(), new ResourcePermissionRequirement());

            result.Succeeded.Should().Be(true);
        }
    }
}
