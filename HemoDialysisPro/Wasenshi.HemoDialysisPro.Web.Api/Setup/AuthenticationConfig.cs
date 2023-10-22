using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api
{
    /// <summary>
    /// Common auth config template for general projects
    /// </summary>
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddAuthenConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthorization(SetupAuthorization)
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Authentication:Issuer"],
                        ValidAudience = configuration["Authentication:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:Key"])),
                        ClockSkew = TimeSpan.Zero
                    };
                    // Sending the access token in the query string is required due to
                    // a limitation in Browser APIs. We restrict it to only calls to the
                    // SignalR hub in this code.
                    // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                    // for more information about security considerations when using
                    // the query string to transmit the access token.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/connect"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie(o =>
                {
                    o.LoginPath = "/Login";
                    o.AccessDeniedPath = "/Login/denied";
                });
            services.Configure<AuthConfig>(configuration.GetSection("Authentication"));

            return services;
        }

        private static void SetupAuthorization(AuthorizationOptions options)
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("backend-admin", c => c
                .RequireAuthenticatedUser()
                .RequireRole(Roles.PowerAdmin)
                .AuthenticationSchemes = new[] { CookieAuthenticationDefaults.AuthenticationScheme });

            options.AddPolicy("bypass", c => c.RequireAssertion(ctx => true));
            options.AddPolicy(Feature.INTEGRATE, c => c.RequireAssertion(ctx => FeatureFlag.HasIntegrated()));
            options.AddPolicy(Feature.MANAGEMENT, c => c.RequireAssertion(ctx => FeatureFlag.HasManagement()));
        }
    }

    public static class Feature
    {
        public const string INTEGRATE = "integrated";
        public const string MANAGEMENT = "management";
    }
}
