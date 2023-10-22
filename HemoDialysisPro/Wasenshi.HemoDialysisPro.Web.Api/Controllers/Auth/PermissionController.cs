using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Utillities;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Role = Wasenshi.HemoDialysisPro.Models.Role;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IRedisClient redis;
        private readonly IUserUnitOfWork uow;

        public PermissionController(UserManager<User> userManager, RoleManager<Role> roleManager, IMapper mapper, IAuthService authService, IRedisClient redis, IUserUnitOfWork uow)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _mapper = mapper;
            _authService = authService;
            this.redis = redis;
            this.uow = uow;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllPermissions()
        {
            static IEnumerable<PermissionInfo> GetPermissions()
            {
                var permissionInfo = typeof(PermissionInfo);
                return permissionInfo
                          .GetFields(BindingFlags.Public | BindingFlags.Static)
                          .Where(f => f.FieldType == typeof(PermissionInfo))
                          .Select(f => (PermissionInfo)f.GetValue(null))
                       .Concat(
                                permissionInfo
                                    .GetNestedTypes()
                                    .SelectMany(x => x.GetFields(BindingFlags.Public | BindingFlags.Static)
                          .Where(f => f.FieldType == typeof(PermissionInfo))
                          .Select(f => (PermissionInfo)f.GetValue(null)))
                       );
            }
            static IEnumerable<PermissionGroupInfo> GetPermissionGroups()
            {
                var permissionGroupInfo = typeof(PermissionGroupInfo);
                return permissionGroupInfo
                            .GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(f => f.FieldType == typeof(PermissionGroupInfo))
                            .Select(f => (PermissionGroupInfo)f.GetValue(null));
            }

            return Ok(new
            {
                Permissions = GetPermissions(),
                Groups = GetPermissionGroups()
            });
        }

        [AllowAnonymous]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserPermissionAsync(Guid id)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(user);

            Permission result = new()
            {
                Permissions = (await userManager.GetClaimsAsync(user)).Select(x => x.Value).Distinct().ToArray(),
                Roles = roles.Except(Roles.AllRoles).ToArray(),
            };

            return Ok(result);
        }

        [PermissionAuthorize("edit-permission")]
        [HttpPost("user/{id}")]
        public async Task<IActionResult> EditUserPermission(Guid id, Permission edit)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await userManager.GetRolesAsync(user);
            var builtInRoles = Roles.AllRoles.Intersect(roles);

            if (!(await _authService.VerifyRoles(edit.Roles)).Succeeded)
            {
                return BadRequest("Invalid role(s)");
            }

            await userManager.RemoveFromRolesAsync(user, roles);
            await userManager.AddToRolesAsync(user, builtInRoles.Concat(edit.Roles));

            var allPermissions = await userManager.GetClaimsAsync(user);
            var removes = allPermissions.Select(x => x.Value).Except(edit.Permissions).ToArray();

            await userManager.RemovePermissionFromUser(user, removes);
            await userManager.AddPermissionToUser(user, edit.Permissions);

            await _authService.RevokeUserAsync(user);
            redis.SetPermissionChangeSignal(user.Id);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("role")]
        public async Task<IActionResult> GetRoles()
        {
            var excluded = Roles.AllRoles.Except(new[] { Roles.Admin }).Select(x => x.ToUpper()).ToList();
            var roles = roleManager.Roles.Where(x => !excluded.Contains(x.NormalizedName)).ToList();

            var result = _mapper.Map<List<RoleViewModel>>(roles);
            for (int i = 0; i < roles.Count; i++)
            {
                var permissions = await roleManager.GetClaimsAsync(roles[i]);
                result[i].Permissions = permissions.Select(x => x.Value).ToList();
            }

            return Ok(result);
        }

        [PermissionAuthorize("role")]
        [HttpPost("role/add")]
        public async Task<IActionResult> AddRole(PermissionRole request)
        {
            var role = new Role(request.RoleName);
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return Problem(result.Errors.First().Description, null, 500);
            }

            await roleManager.AddPermissionToRole(role, request.Permissions);
            var roleResult = _mapper.Map<RoleViewModel>(role);
            roleResult.Permissions = request.Permissions;

            return Ok(roleResult);
        }

        [PermissionAuthorize("role")]
        [HttpPost("role/{id}")]
        public async Task<IActionResult> EditRole(Guid id, PermissionRole edit)
        {
            var excluded = Roles.AllRoles.Except(new[] { Roles.Admin }).Select(x => x.ToUpper()).ToList();
            var role = roleManager.Roles.FirstOrDefault(y => y.Id == id);
            if (role == null)
            {
                return NotFound();
            }
            if (excluded.Contains(role.NormalizedName))
            {
                return Forbid("Cannot edit built-in role");
            }

            if (role.NormalizedName != Roles.Admin.Normalize())
            {
                role.Name = edit.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return Problem(result.Errors.First().Description, null, 500);
                }
                await roleManager.UpdateNormalizedRoleNameAsync(role);
            }

            var allPermissions = await roleManager.GetClaimsAsync(role);
            var removes = allPermissions.Select(x => x.Value).Except(edit.Permissions).ToArray();

            await roleManager.RemovePermissionFromRole(role, removes);
            await roleManager.AddPermissionToRole(role, edit.Permissions);

            // set permission changes to all assigned user
            var userList = uow.Role.GetUserRolesMap().Where(x => x.RoleId == id).Select(x => x.UserId).ToList();
            foreach (var user in userList)
            {
                redis.SetPermissionChangeSignal(user);
            }

            return Ok();
        }

        [PermissionAuthorize("role")]
        [HttpDelete("role/{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var excluded = Roles.AllRoles.Select(x => x.Normalize()).ToList();
            var role = roleManager.Roles.FirstOrDefault(y => y.Id == id);
            if (role == null)
            {
                return NotFound();
            }
            if (excluded.Contains(role.NormalizedName))
            {
                return Forbid("Cannot delete built-in role");
            }

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return Problem(result.Errors.First().Description, null, 500);
            }

            return Ok();
        }

    }
}
