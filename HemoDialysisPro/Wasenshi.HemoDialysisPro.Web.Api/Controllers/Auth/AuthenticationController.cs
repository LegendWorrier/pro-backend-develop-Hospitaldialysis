using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Serilog;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Helpers;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Microsoft.Extensions.Logging;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IUserInfoService userInfoService;
        private readonly ISmartLoginService smartLogin;
        private readonly AuthHelper helper;
        private readonly IStringLocalizer<ShareResource> localizer;
        private readonly IEnumerable<IAuthHandler> authPlugins;
        private readonly IRedisClient redis;

        public AuthenticationController(
            IMapper mapper,
            IAuthService authService,
            IUserInfoService userInfoService,
            ISmartLoginService smartLogin,
            AuthHelper helper,
            IStringLocalizer<ShareResource> localizer,
            IEnumerable<IAuthHandler> authPlugins,
            IRedisClient redis)
        {
            _mapper = mapper;
            _authService = authService;
            this.userInfoService = userInfoService;
            this.smartLogin = smartLogin;
            this.helper = helper;
            this.localizer = localizer;
            this.authPlugins = authPlugins;
            this.redis = redis;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LogIn([FromBody] LoginViewModel login)
        {
            UserResult result = null;
            var pluginResult = await authPlugins.ExecutePlugins<IActionResult, IAuthHandler>(async handler =>
            {
                if (handler.OnLogIn != null)
                {
                    var tmp = await handler.OnLogIn(login.Username, login.Password);
                    if (tmp != null)
                    {
                        result = userInfoService.FindUser(x => x.UserName == tmp.User.UserName);
                        if (result == null)
                        {
                            if (!(tmp.User.Units ??= new List<UserUnit>()).Any())
                            {
                                var defaultUnitId = redis.GetMonitorPool().UnitListFromCache().First().Id;
                                tmp.User.Units.Add(new UserUnit { UnitId = defaultUnitId });
                            }
                            var regisResult = await _authService.RegisterUserAsync(_mapper.Map<User>(tmp.User), login.Password, tmp.Roles);
                            if (!regisResult.Succeeded)
                            {
                                return BadRequest(regisResult.Errors);
                            }
                            result = tmp;
                        }
                        var token = await helper.LoginAndGetToken(result);
                        return Ok(new
                        {
                            access_token = token.AccessToken,
                            expires_in = token.Expire,
                            expired_mode = LicenseManager.ExpiredMode,
                            feature = LicenseManager.FeatureList
                        });
                    }
                }

                return null;
            }, e => {
                Log.Error(e, "Plugin error at log in");
            });

            if (pluginResult != null)
            {
                return pluginResult;
            }

            result = await _authService.AuthenticateAsync(login.Username, login.Password);

            if (result == null)
            {
                return BadRequest(string.Format(localizer["UserNameNotFound"], login.Username));
            }

            if (result.User != null)
            {
                var token = await helper.LoginAndGetToken(result);
                return Ok(new
                {
                    access_token = token.AccessToken,
                    expires_in = token.Expire,
                    expired_mode = LicenseManager.ExpiredMode,
                    feature = LicenseManager.FeatureList
                });
            }

            return BadRequest(localizer["PasswordMismatch"].Value);
        }

        [AllowAnonymous]
        [HttpPost("smart-login")]
        public async Task<IActionResult> LogInWithToken([FromBody] string smartToken)
        {
            UserResult result = await smartLogin.LoginWithToken(smartToken);

            if (result == null)
            {
                return BadRequest("Token is invalid.");
            }

            var token = await helper.LoginAndGetToken(result);
            return Ok(new
            {
                access_token = token.AccessToken,
                expires_in = token.Expire,
                expired_mode = LicenseManager.ExpiredMode,
                feature = LicenseManager.FeatureList
            });
        }

        [AllowAnonymous]
        [HttpPost("one-time-token")]
        public async Task<IActionResult> LogInWithOneTimeToken([FromBody] string oneTimeToken)
        {
            UserResult result = await smartLogin.LoginWithOneTimeToken(oneTimeToken);

            if (result == null)
            {
                return BadRequest("Token is invalid.");
            }

            var token = await helper.LoginAndGetToken(result);

            return Ok(new
            {
                access_token = token.AccessToken,
                expires_in = token.Expire,
                expired_mode = LicenseManager.ExpiredMode,
                feature = LicenseManager.FeatureList
            });
        }

        [PermissionAuthorize(Permissions.User.ADD_PERMISSION)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel register)
        {
            var user = _mapper.Map<RegisterViewModel, User>(register);
            var roles = new List<string> { register.Role };
            if (register.isAdmin)
            {
                roles.Add(Roles.Admin);
            }

            if (!_authService.VerifyUnit(User, register.Units))
            {
                return Forbid();
            }

            var pluginResult = await authPlugins.ExecutePlugins(async handler =>
            {
                var authResult = await handler.OnRegister(user, register.Password);
                if (!authResult.Success)
                {
                    return authResult;
                }
                return null;
            }, e => Log.Error(e, "Plugin error at user registration."));
            if (pluginResult != null) { return Problem(pluginResult.ErrorDetail, null, 500); }

            IdentityResult result = await _authService.RegisterUserAsync(user, register.Password, roles);

            if (result.Succeeded)
            {
                return Created(string.Empty, user.Id);
            }

            return Problem(result.Errors.First().Description, null, 500);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var userId = User.GetUserIdAsGuid();
            if (redis.CheckPermissionChangeSignal(userId))
            {
                return Unauthorized(new {
                    code = "permission-change",
                    message = "Your user's permission has been changed. Please re-login again." 
                });
            }

            var refreshToken = Request.Cookies["refreshToken"];
            var ip = helper.GetIpAddress();
            Log.Information($"client ip: {ip}");
            var token = await _authService.RefreshToken(refreshToken, ip);

            if (token == null)
                return Unauthorized(new { message = "Invalid token" });

            helper.SetTokenCookie(token.RefreshToken);
            helper.SetLangCookie();

            return Ok(new
            {
                access_token = token.AccessToken,
                expires_in = token.Expire,
                expired_mode = LicenseManager.ExpiredMode,
                feature = LicenseManager.FeatureList
            });
        }

        [AllowAnonymous]
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var ip = helper.GetIpAddress();
            Log.Information($"client ip: {ip}");
            var response = _authService.RevokeToken(token, ip);

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }

        
    }
}
