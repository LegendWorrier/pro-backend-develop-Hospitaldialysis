using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.PluginBase;

namespace Wasenshi.HemoDialysisPro.Utils
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                string code = "UNKNOWN";

                void customCode(string customCode) {
                    code = customCode;

                    if (customCode == "UNAUTHORIZED") // special code, forcing FE to refresh token.
                    {
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                    }
                }

                switch (error)
                {
                    case AppException e:
                        // custom application error
                        customCode(e.Code);
                        break;
                    case PluginException pe:
                        // custom error from plugin
                        customCode(pe.Code);
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                if (response.StatusCode == 404)
                {
                    code = "NOTFOUND";
                }

                var result = JsonSerializer.Serialize(
                    new
                    {
                        Code = code,
                        error?.Message
                    }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (response.StatusCode != (int)HttpStatusCode.Conflict)
                {
                    logger.LogError(new EventId(13, "ApplicationError"), error, "An unhandled exception was thrown.");
                }
                await response.WriteAsync(result);
            }
        }
    }
}
