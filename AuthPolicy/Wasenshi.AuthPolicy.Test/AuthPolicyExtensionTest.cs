using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Test.Base;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class AuthPolicyExtensionTest : ControllerTestBase
    {

        TestOwnerHandler mockHandler;

        protected override void InitAndConfig(ServiceCollection services)
        {
            mockHandler = new TestOwnerHandler();

            services.Remove(_services.FirstOrDefault(n => n.ImplementationType == typeof(TestOwnerHandler)));
            services.Remove(_services.FirstOrDefault(n => n.ImplementationType == typeof(ResourcePermissionHandlerDefault)));
            services.AddSingleton<IAuthorizationHandler>(mockHandler);
        }

        [Fact]
        public async Task WhenCallValidatePermission_ShouldValidateSuccessfully()
        {
            //Arrange
            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController();
            mockHandler.isPass = true;

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test(model), _app.ApplicationServices);

            resultContext.Exception.Should().BeNull($"error: {resultContext.Exception}");
            resultContext.Result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task WhenCallValidatePermission_Fail_ShouldReturnForbidResult()
        {
            //Arrange

            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController();
            mockHandler.isPass = false;

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test(model), _app.ApplicationServices);

            resultContext.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task WhenCall_WithOnePolicy_ShouldOverrideDefaultPolicySuccessfully()
        {
            //Arrange
            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController();

            int numberOfCalled = 0;
            mockHandler.SetupCallback((context, requirement, _) =>
            {
                requirement.PermissionPolicyName.Should().BeOneOf("ALTERNATE");
                numberOfCalled++;
            });

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test2(model), _app.ApplicationServices);
            numberOfCalled.Should().Be(1);
        }

        [Fact]
        public async Task WhenCall_WithAdditionalPolicy_ShouldIncludeDefaultPolicySuccessfully()
        {
            //Arrange
            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController();

            int numberOfCalled = 0;
            mockHandler.SetupCallback((context, requirement, _) =>
            {
                requirement.PermissionPolicyName.Should().BeOneOf("DOUBLE", "");
                numberOfCalled++;
            });

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test3(model), _app.ApplicationServices);
            if (resultContext.Exception != null)
            {
                throw resultContext.Exception;
            }
            numberOfCalled.Should().Be(2);
        }

        [Fact]
        public async Task WhenCall_WithControllerPolicy_ShouldRunPolicySuccessfully()
        {
            //Arrange
            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController2();

            int numberOfCalled = 0;
            mockHandler.SetupCallback((context, requirement, _) =>
            {
                requirement.PermissionPolicyName.Should().BeOneOf("CONTROLLER");
                numberOfCalled++;
            });

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test(model), _app.ApplicationServices);
            if (resultContext.Exception != null)
            {
                throw resultContext.Exception;
            }
            numberOfCalled.Should().Be(1);
        }

        [Fact]
        public async Task WhenCall_WithCombinationPolicy_ShouldRunAllPolicySuccessfully()
        {
            //Arrange
            var model = new TestModel
            {
                Password = "haha",
                Username = "test"
            };

            var controller = new TestController2();

            int numberOfCalled = 0;
            mockHandler.SetupCallback((context, requirement, _) =>
            {
                requirement.PermissionPolicyName.Should().BeOneOf("CONTROLLER", "ALTERNATE2", "ALTERNATE3");
                numberOfCalled++;
            });

            //Act
            var resultContext = await controller.ExecuteAction(c => c.Test2(model), _app.ApplicationServices);
            if (resultContext.Exception != null)
            {
                throw resultContext.Exception;
            }
            numberOfCalled.Should().Be(3);
        }

        
        //========================================== Test Controllers ==================================================================
        class TestController : ControllerBase
        {
            public async Task<IActionResult> Test(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }


            [ResourcePermissionPolicy("alternate")]
            public async Task<IActionResult> Test2(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }

            [ResourcePermissionPolicy(null)]
            [ResourcePermissionPolicy("Double")]
            public async Task<IActionResult> Test3(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }
        }

        [ResourcePermissionPolicy("Controller")]
        class TestController2 : ControllerBase
        {
            public async Task<IActionResult> Test(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }

            [ResourcePermissionPolicy("alternate2")]
            [ResourcePermissionPolicy("alternate3")]
            public async Task<IActionResult> Test2(TestModel model)
            {
                await this.ValidateResourcePermissionAsync(model);

                return Ok();
            }
        }
    }
}
