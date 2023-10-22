﻿using Amazon.Runtime;
using AWS.Logger;
using AWS.Logger.SeriLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace Wasenshi.HemoDialysisPro.Report.Api
{
    public static class LogConfig
    {
        private const string DEFAULT_LOCAL_SEQ = "http://localhost:5341";
        private const int BODY_LOG_SIZE_LIMIT = 262144; // ~260 kb

        public static void InitLog()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? DEFAULT_LOCAL_SEQ)
                .WriteTo.Console()
                .CreateLogger();
        }

        public static IApplicationBuilder UseLogging(this IApplicationBuilder application)
        {
            application
                .UseMiddleware<LoggingMiddleware>()
                .UseSerilogRequestLogging(o =>
                {
                    o.EnrichDiagnosticContext = Enriching;
                });

            return application;
        }

        public static async void Enriching(IDiagnosticContext diagnosticContext, HttpContext context)
        {
            // Add platform detection
            diagnosticContext.Set("Platform", context.Request.Headers["Platform"]);

            if (context.Request.Body.Length < BODY_LOG_SIZE_LIMIT)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024 * 1000,
                    leaveOpen: true);
                context.Request.Body.Position = 0;
                string request = null;
                try
                {
                    request = reader.ReadToEndAsync().GetAwaiter().GetResult();
                }
                catch (BadHttpRequestException) { /* filtering out random exception from the framework */ }
                catch (Exception e)
                {
                    request = $"error reading request body: {e}";
                }

                diagnosticContext.Set("RequestBody", request);
            }
            else
            {
                diagnosticContext.Set("RequestBody", "Too large or is multipart.");
            }

            try
            {
                if (context.Response.Body.CanRead &&
                    context.Response.Body.Length < BODY_LOG_SIZE_LIMIT)
                {
                    context.Response.Body.Position = 0;
                    var reader = new StreamReader(context.Response.Body);
                    string response = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    diagnosticContext.Set("ResponseBody", response);
                }
                else
                {
                    diagnosticContext.Set("ResponseBody", "Too large or cannot read.");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in response body log.");
            }

        }

        /// <summary>
        /// This middleware helps logging to be able to capture Response Body.
        /// It buffers the body, let other middlewares use and read the body and then re-assign the original body for output.
        /// </summary>
        public class LoggingMiddleware
        {
            private readonly RequestDelegate _next;

            public LoggingMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                context.Request.EnableBuffering(1024 * 1000);
                Stream original = context.Response.Body;

                try
                {
                    using var stream = new MemoryStream();
                    context.Response.Body = stream;
                    await _next.Invoke(context);

                    stream.Position = 0;
                    await stream.CopyToAsync(original);
                }
                finally
                {
                    context.Response.Body = original;
                }
            }
        }
    }
}
