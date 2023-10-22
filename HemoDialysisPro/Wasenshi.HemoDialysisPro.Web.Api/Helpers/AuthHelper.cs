using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System;
using ServiceStack.Redis;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Serilog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models;
using System.Diagnostics;

namespace Wasenshi.HemoDialysisPro.Web.Api.Helpers
{
    public class AuthHelper
    {
        private readonly IAuthService authService;
        private readonly IRedisClient redis;
        private readonly IHttpContextAccessor accessor;
        private readonly bool isLocalServer;

        public AuthHelper(IAuthService authService, IRedisClient redis, IHttpContextAccessor accessor, IConfiguration config)
        {
            this.authService = authService;
            this.redis = redis;
            this.accessor = accessor;
            isLocalServer = config.GetValue<bool>("Authentication:Local");
        }

        public async Task<AuthResponse> LoginAndGetToken(UserResult userResult)
        {
            Debug.Assert(userResult != null);
            var ip = GetIpAddress();
            Log.Information($"client ip: {ip}");
            var token = await authService.GenerateToken(userResult, ip);
            SetTokenCookie(token.RefreshToken);
            SetLangCookie();
            ResetLoginSignal(userResult.User.Id);

            return token;
        }

        public void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
#if !DEBUG
                Secure = true,
#endif
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            accessor.HttpContext.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public void SetLangCookie()
        {
            accessor.HttpContext.Response.SetLangCookie();
        }

        public void ResetLoginSignal(Guid userId)
        {
            redis.ResetPermissionChangeSignal(userId);
        }

        public string GetIpAddress()
        {
            string remoteIp = accessor.HttpContext.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            Log.Debug($"direct remote ip: {remoteIp}");
            // Remember: the longer and more specific expression, the better regex it will be.
            const string ip4 = @"^([0-9]{1,3}\.){3}[0-9]{1,3}$";
            const string ip6 = @"^" +
                    @"([0-9a-fA-F]{1,4}::?){7}[0-9a-fA-F]{1,4}|" +
                    @"([0-9a-fA-F]{1,4}:){1,4}:" +
                    @"((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3}" +
                    @"(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])" +
                    @"$";

            if (accessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                Log.Information($"forwarded ip: {accessor.HttpContext.Request.Headers["X-Forwarded-For"]}");
                var ipList = accessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString().Split(",").Select(x => x.Trim());
                var clientIp = ipList.FirstOrDefault(x =>
                                        (Regex.IsMatch(x, ip4, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(10)) && (isLocalServer || !IsLocalIp(x))) ||
                                        Regex.IsMatch(x, ip6, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(10)));
                if (clientIp == null) // Give priority and allow for false-case user rather than deny possible hack here : if occur, investigate more
                {
                    if (!isLocalServer)
                    {
                        Log.Warning("Potential security breach/hack: Cannot retreive valid clientIp from X-Forwarded-For header (need further investigation immediately)");
                    }
                    clientIp = remoteIp;
                }
                // Extra check
                else if (clientIp.Split("::").Length > 2)
                {
                    // invalid ip6
                    Log.Warning("Potential security breach/hack: Invalid IPv6 format (need furhter investigation immediately)");
                }
                return clientIp;
            }
            else // Give priority and allow for false-case user rather than deny possible hack here : if occur, investigate more
            {
                if (!isLocalServer)
                {
                    Log.Warning("Potential security breach/hack: Cannot retreive valid clientIp from X-Forwarded-For header (need further investigation immediately)");
                }
                return remoteIp;
            }
        }

        public bool IsLocalIp(string ip)
        {
            return ip.StartsWith("172") || ip.StartsWith("168") || ip.StartsWith("192");
        }
    }
}
