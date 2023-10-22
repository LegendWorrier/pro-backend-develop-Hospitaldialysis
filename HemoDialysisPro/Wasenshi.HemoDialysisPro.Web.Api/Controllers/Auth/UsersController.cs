using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Users;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Serilog;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService userManage;
        private readonly IUserInfoService userInfo;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IEnumerable<IAuthHandler> authPlugins;
        private readonly IDistributedCache _distributedCache;

        public UsersController(
            IUserManagementService userManage,
            IUserInfoService userInfo,
            IMapper mapper,
            IAuthService authService,
            IEnumerable<IAuthHandler> authPlugins,
            IDistributedCache distributedCache)
        {
            this.userManage = userManage;
            this.userInfo = userInfo;
            _mapper = mapper;
            _authService = authService;
            this.authPlugins = authPlugins;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = userInfo.GetAllUsers(User.GetUnitFilter<IUser>(x => x.Units.Select(u => u.UnitId)));
            return Ok(_mapper.Map<IEnumerable<PwdHashHiddenResult>>(users));
        }

        [HttpGet("doctors")]
        public IActionResult GetDoctorList(int unitId)
        {
            int[] unitList = null;
            if (!User.IsInRole(Roles.PowerAdmin))
            {
                if (unitId != 0 && !User.GetUnitList().Contains(unitId))
                {
                    return Forbid();
                }

                if (unitId == 0)
                {
                    unitList = User.GetUnitList().ToArray();
                }
                else
                {
                    unitList = new[] { unitId };
                }
            }

            var doctors = userInfo.GetDoctorList(unitList);
            return Ok(_mapper.Map<IEnumerable<UserPwdHashHidden>>(doctors));
        }

        [HttpGet("nurses")]
        public IActionResult GetNurseList(int unitId)
        {
            int[] unitList = null;
            if (!User.IsInRole(Roles.PowerAdmin))
            {
                if (unitId != 0 && !User.GetUnitList().Contains(unitId))
                {
                    return Forbid();
                }

                if (unitId == 0)
                {
                    unitList = User.GetUnitList().ToArray();
                }
                else
                {
                    unitList = new[] { unitId };
                }
            }

            var nurses = userInfo.GetNurseList(unitList);
            return Ok(_mapper.Map<IEnumerable<PwdHashHiddenResult>>(nurses));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserAsync(Guid id, bool showPwdHash = false)
        {
            var userResult = await userManage.GetUserAsync(id);
            if (userResult == null)
            {
                return NotFound("User not found.");
            }

            if (User.IsInRole(Roles.PowerAdmin) && showPwdHash)
            {
                return Ok(userResult);
            }

            return Ok(_mapper.Map<PwdHashHiddenResult>(userResult));
        }

        [HttpPost("{id}/edit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditUserWithSignature(Guid id, [FromForm] IFormCollection data)
        {
            var signature = data.Files.FirstOrDefault();
            if (signature == null)
            {
                return BadRequest("Please upload signature image file.");
            }

            var editUser = JsonSerializer.Deserialize<EditUserViewModel>(data["application/json"], new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            });

            await this.ValidateFieldsAsync(editUser);

            (User user, IActionResult error) = await _EditUser_ValidateAndMapDataAsync(id, editUser);

            if (error != null)
            {
                return error;
            }

            var pluginResult = await authPlugins.ExecutePlugins(async item =>
            {
                var authResult = await item.OnEdit(user.UserName, user);
                if (!authResult.Success)
                {
                    return authResult;
                }
                return null;
            }, e => Log.Error(e, "Plugin error on user edit"));
            if (pluginResult != null) { return BadRequest(pluginResult.ErrorDetail); }

            IdentityResult result = await userManage.EditUserAsync(user, editUser.Password, signature);

            if (result.Succeeded)
            {
                await _distributedCache.RemoveAsync($"user:{id}");
                return Ok(new { Signature = user.Signature });
            }

            return Problem(result.Errors.First().Description, null, 500);
        }

        [FieldAuthorize]
        [HttpPost("{id}/edit")]
        [Consumes("application/json")]
        public async Task<IActionResult> EditUser(Guid id, [FromBody] EditUserViewModel editUser)
        {
            (User user, IActionResult error) = await _EditUser_ValidateAndMapDataAsync(id, editUser);

            if (error != null)
            {
                return error;
            }

            var pluginResult = await authPlugins.ExecutePlugins(async item =>
            {
                var authResult = await item.OnEdit(user.UserName, user);
                if (!authResult.Success)
                {
                    return authResult;
                }
                return null;
            }, e => Log.Error(e, "Plugin error on user edit"));
            if (pluginResult != null) { return BadRequest(pluginResult.ErrorDetail); }

            IdentityResult result = await userManage.EditUserAsync(user, editUser.Password);

            if (result.Succeeded)
            {
                await _distributedCache.RemoveAsync($"user:{id}");
                return Ok();
            }

            return Problem(result.Errors.First().Description, null, 500);
        }

        private async Task<(User, IActionResult)> _EditUser_ValidateAndMapDataAsync(Guid id, EditUserViewModel editData)
        {
            UserResult userResult = await userManage.GetUserAsync(id);

            if (userResult == null)
            {
                return (null, NotFound("User not found."));
            }

            await this.ValidateResourcePermissionAsync(userResult.User);

            if (!_authService.VerifyUnit(User, editData.Units))
            {
                return (null, Forbid());
            }
            // Safe-Guard: cannot directly edit your own password without knowing current password
            var userId = User.GetUserId();
            if (new Guid(userId) == id && !string.IsNullOrWhiteSpace(editData.Password))
            {
                return (null, BadRequest(new { Error = "Cannot edit password here. Go to your profile instead." }));
            }

            User user = _mapper.Map(editData, userResult.User as User);

            return (user, null);
        }

        [PermissionAuthorize(Permissions.User.EDIT_PERMISSION)]
        [HttpPost("{id}/changerole")]
        public async Task<IActionResult> ChangeRoles(Guid id, [FromBody] List<string> newRoles)
        {
            UserResult userResult = await userManage.GetUserAsync(id);

            if (userResult == null)
            {
                return NotFound("User not found.");
            }

            await this.ValidateResourcePermissionAsync(userResult.User);

            IdentityResult result = await userManage.ChangeUserRolesAsync(userResult.User as User, newRoles);

            if (result.Succeeded)
            {
                await _distributedCache.RemoveAsync($"user:{userResult.User.Id}");
                return Ok();
            }

            return Problem(result.Errors.First().Description, null, 500);
        }

        [PermissionAuthorize(Permissions.User.DEL_PERMISSION)]
        [ResourcePermissionPolicy("delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            UserResult userResult = await userManage.GetUserAsync(id);

            if (userResult == null)
            {
                return NotFound("User not found.");
            }

            await this.ValidateResourcePermissionAsync(userResult.User);

            var pluginResult = await authPlugins.ExecutePlugins(async item =>
            {
                var authResult = await item.OnDelete(userResult.User.UserName);
                if (!authResult.Success)
                {
                    return authResult;
                }
                return null;
            }, e => Log.Error(e, "Plugin error on user delete"));
            if (pluginResult != null) { return BadRequest(pluginResult.ErrorDetail); }

            IdentityResult result = await userManage.DeleteUser(id);

            if (result.Succeeded)
            {
                await _distributedCache.RemoveAsync($"user:{userResult.User.Id}");
                return Ok();
            }

            return Problem(result.Errors.First().Description, null, 500);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(EditPasswordViewModel request)
        {
            UserResult userResult = await userManage.GetUserAsync(new Guid(User.GetUserId()));

            if (userResult == null)
            {
                return NotFound("User not found.");
            }

            await this.ValidateResourcePermissionAsync(userResult.User);

            var pluginResult = await authPlugins.ExecutePlugins(async item =>
            {
                var authResult = await item.OnChangePassword(userResult.User.UserName, request.OldPassword, request.NewPassword);
                if (!authResult.Success)
                {
                    return authResult;
                }
                return null;
            }, e => Log.Error(e, "Plugin error on user change password"));
            if (pluginResult != null) { return BadRequest(pluginResult.ErrorDetail); }

            IdentityResult result = await userManage.ChangePasswordAsync(userResult.User as User, request.OldPassword, request.NewPassword);

            if (result.Succeeded)
            {
                await _distributedCache.RemoveAsync($"user:{userResult.User.Id}");
                return Ok();
            }

            return Problem(result.Errors.First().Description, null, 500);
        }
    }
}
