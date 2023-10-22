using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api
{
    public class APIBlockMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<APIBlockMiddleware> logger;
        private readonly IServiceProvider service;

        public APIBlockMiddleware(RequestDelegate next, ILogger<APIBlockMiddleware> logger, IServiceProvider service)
        {
            _next = next;
            this.logger = logger;
            this.service = service;
        }

        public async Task Invoke(HttpContext context)
        {
            if (FeatureFlag.IsDisabled())
            {
                throw new AppException("LICENSE_ERROR", "The license is expired or is invalid.");
            }

            if (!context.Request.Path.StartsWithSegments("/api/authentication"))
            {
                using var scope = service.CreateScope();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                if (redis.CheckPermissionChangeSignal(context.User.GetUserIdAsGuid()))
                {
                    throw new AppException("PERMISSION_CHANGE", "Your user's permission has been changed. Please re-login again.");
                }
            }

            await _next(context);
        }

        public static async Task CheckAvailibility(HttpContext context)
        {
            var available = !FeatureFlag.IsDisabled();

            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = 200;

            var result = JsonSerializer.Serialize(available, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
    }
}
