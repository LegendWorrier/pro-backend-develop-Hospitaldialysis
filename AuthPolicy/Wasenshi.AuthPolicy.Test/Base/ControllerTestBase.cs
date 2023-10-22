using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Filter;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using FluentAssertions.Equivalency;
using Microsoft.VisualBasic;

namespace Wasenshi.AuthPolicy.Test.Base
{
    public abstract class ControllerTestBase
    {
        protected ServiceCollection _services;
        protected ApplicationBuilder _app;

        public ControllerTestBase()
        {
            _services = new ServiceCollection();
            _services
                .AddLogging()
                .AddAuthorizationCore()
                .AddPolicy();

            InitAndConfig(_services);

            _app = new ApplicationBuilder(_services.BuildServiceProvider());
        }

        protected virtual void InitAndConfig(ServiceCollection services)
        {

        }

        
    }

    public static class ControllerExtension
    {
        //=========================================== Utilities ==========================================================
        private static (ControllerActionDescriptor descriptor, object[] args) CreateActionDescriptorAndArgs<T>(T controller, Expression<Func<T, Task<IActionResult>>> action) where T : ControllerBase
        {
            var type = controller.GetType();
            var actionType = action.Body as MethodCallExpression;
            var args = actionType.Arguments.Select(x => {
                if (x is MemberExpression memberAccess)
                {
                    var body = (memberAccess.Expression as ConstantExpression).Value;
                    var method = memberAccess.Member;

                    switch (method.MemberType)
                    {
                        case MemberTypes.Field:
                            return ((FieldInfo)method).GetValue(body);
                        case MemberTypes.Property:
                            return ((PropertyInfo)method).GetValue(body);
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (x is ConstantExpression constant)
                {
                    return constant.Value;
                }

                throw new NotImplementedException();
            }).ToArray();
            return (new ControllerActionDescriptor
            {
                ActionName = actionType.Method.Name,
                ControllerName = type.Name,
                ControllerTypeInfo = type.GetTypeInfo(),
                MethodInfo = actionType.Method,
                DisplayName = actionType.Method.Name,
                Parameters = action.Parameters.Select(x => new ParameterDescriptor
                {
                    Name = x.Name,
                    ParameterType = x.Type
                }).ToList()
            }, args);
        }

        public static async Task<ActionExecutedContext> ExecuteAction<T>(this T controller, Expression<Func<T, Task<IActionResult>>> action, IServiceProvider services, ClaimsPrincipal user = null) where T : ControllerBase
        {
            var args = controller.PrepareTestController(services, action, user);
            return await controller.ExecuteTestActionWithFilterAsync(args);
        }

        private static object[] PrepareTestController<T>(this T controller, IServiceProvider services, Expression<Func<T, Task<IActionResult>>> action, ClaimsPrincipal user = null) where T : ControllerBase
        {
            user ??= new Mock<ClaimsPrincipal>().Object;
            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.RequestServices = services;
            services.GetService<IHttpContextAccessor>().HttpContext = httpContext;

            (var descriptor, var args) = CreateActionDescriptorAndArgs(controller, action);
            var actionContext= new ActionContext(httpContext, new RouteData(), descriptor);
            controller.ControllerContext = new ControllerContext(actionContext);
            return args;
        }

        private static async Task<ActionExecutedContext> ExecuteTestActionWithFilterAsync<T>(this T controller, params object[] args) where T : ControllerBase
        {
            var actionContext = controller.ControllerContext;
            var metadata = new List<IFilterMetadata>();
            var ctx = new ActionExecutedContext(actionContext, metadata, controller);
            var actionExecutingContext = new ActionExecutingContext(actionContext, metadata, new Dictionary<string, object>(), controller);
            ActionExecutionDelegate next = async () =>
            {
                IActionResult result = null;
                try
                {
                    var expression = Expression.Call(Expression.Constant(controller), actionContext.ActionDescriptor.MethodInfo, args.Select(x => Expression.Constant(x)));
                    result = await Expression.Lambda<Func<Task<IActionResult>>>(expression).Compile().Invoke();
                }
                catch (Exception ex)
                {
                    ctx.Exception = ex;
                }
                ctx.Result = result;

                return ctx;
            };
            AuthorizationFilter mainFilter = new AuthorizationFilter();
            PermissionFilter permissionFilter = ActivatorUtilities.CreateInstance<PermissionFilter>(controller.HttpContext.RequestServices);
            //Actuall execution
            ActionExecutionDelegate permissionCheck = async () => {
                try
                {
                    await permissionFilter.OnActionExecutionAsync(actionExecutingContext, next);
                }
                catch (Exception ex)
                {
                    ctx.Exception = ex;
                }
                return ctx;
            };
            await mainFilter.OnActionExecutionAsync(actionExecutingContext, permissionCheck);

            return ctx;
        }
    }
}
