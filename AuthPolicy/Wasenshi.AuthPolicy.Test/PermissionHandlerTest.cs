using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Test.Base;
using Wasenshi.AuthPolicy.Utillities;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class PermissionHandlerTest : ControllerTestBase
    {
        private ClaimsPrincipal GetUser(string userId, params string[] permissions)
        {
            var claims = permissions.Select(x => new Claim(ClaimsPermissionHelper.PERMISSION_TYPE, x)).ToList();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        [Fact]
        public async Task Permission_ShouldBlockSuccessfully()
        {
            var controller = new TestController();

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test(), _app.ApplicationServices, GetUser("TestUser"));
            // unprotected should not be affected

            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Permission1(), _app.ApplicationServices, GetUser("TestUser"));
            // protected should block successfully

            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Permission_ShouldValidateSuccessfully()
        {

            var controller = new TestController();

            // has only permission 1
            var user = GetUser("TestUser", "permission1");

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_Permission1(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            var model = new TestModel { id = "test" };
            resultContext = await controller.ExecuteAction(c => c.Test_Permission2(model), _app.ApplicationServices, user);
            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();

            // has both permission 1 & 2
            user = GetUser("TestUser", "permission1", "permission2");
            resultContext = await controller.ExecuteAction(c => c.Test_Permission2(model), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Permission_ShouldAllowEither_Controller_Or_Method_Permission()
        {

            var controller = new TestController2();

            // has only controller level permission
            var user = GetUser("TestUser", "controller-global-permission");

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_Controller_Global(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Detail(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            // has only detail level permission
            user = GetUser("TestUser", "detail-permission");
            resultContext = await controller.ExecuteAction(c => c.Test_Detail(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Controller_Global(), _app.ApplicationServices, user);
            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task Permission_ShouldPrioritize_MethodPermission()
        {

            var controller = new TestController2();

            // has only controller level permission
            var user = GetUser("TestUser", "controller-global-permission");

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_Controller_Global(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Priority(), _app.ApplicationServices, user);
            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();

            // has only detail level permission
            user = GetUser("TestUser", "priority-permission");
            resultContext = await controller.ExecuteAction(c => c.Test_Priority(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Controller_Global(), _app.ApplicationServices, user);
            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GlobalPermission_ShouldBypass_even_required_permission()
        {
            var controller = new TestController2();

            // has global permission
            var user = GetUser("TestUser", "global");
            _app.ApplicationServices.GetService<IOptionsMonitor<AuthPolicyOptions>>().CurrentValue.GlobalPermission = "global";

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_Priority(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task AlternatePermission_ShouldBypass_And_IsOptional()
        {
            var controller = new TestController2();

            // has detail permission
            var user = GetUser("TestUser", "detail-permission");

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_Detail(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            resultContext = await controller.ExecuteAction(c => c.Test_Priority(), _app.ApplicationServices, user);
            resultContext.Exception.Should().NotBeNull("Should be blocked");
            resultContext.Result.Should().BeOfType<ForbidResult>();

            // add alternate permission
            user = GetUser("TestUser", "alternate-bypass");
            resultContext = await controller.ExecuteAction(c => c.Test_Priority(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Permission_ShouldValidate_Alternate_Successfully()
        {

            var controller = new TestController();

            // has only permission 1
            var user = GetUser("TestUser", "permission1");

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test_AlternatePermission(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();

            // has only permission 2
            user = GetUser("TestUser", "permission2");

            //Act
            resultContext = await controller.ExecuteAction(c => c.Test_AlternatePermission(), _app.ApplicationServices, user);
            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();
        }

        // ========================== Test Controllers =============================
        class TestController : ControllerBase
        {
            public async Task<IActionResult> Test()
            {
                return Ok();
            }

            [PermissionAuthorize("permission1")]
            public async Task<IActionResult> Test_Permission1()
            {
                return Ok();
            }

            [PermissionAuthorize("permission2")]
            public async Task<IActionResult> Test_Permission2(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }

            [PermissionAuthorize("permission1, permission2")]
            public async Task<IActionResult> Test_AlternatePermission()
            {

                return Ok();
            }
        }

        [PermissionAuthorize("alternate-bypass", true)]
        [PermissionAuthorize("controller-global-permission")]
        class TestController2 : ControllerBase
        {
            public async Task<IActionResult> Test_Controller_Global()
            {

                return Ok();
            }

            [PermissionAuthorize("detail-permission")]
            public async Task<IActionResult> Test_Detail()
            {

                return Ok();
            }

            [PermissionAuthorize("priority-permission", true)]
            public async Task<IActionResult> Test_Priority()
            {

                return Ok();
            }
        }

    }
}
