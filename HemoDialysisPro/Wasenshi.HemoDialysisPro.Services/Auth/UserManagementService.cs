using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserUnitOfWork _userUow;
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        private readonly IUploadService _upload;

        public UserManagementService(IUserUnitOfWork userUow, UserManager<User> userManager, IAuthService authService, IUploadService upload)
        {
            _userUow = userUow;
            _userManager = userManager;
            _authService = authService;
            _upload = upload;
        }

        public async Task<IdentityResult> EditUserAsync(User userEdit, string password, IFormFile signature)
        {
            IdentityResult result = await _EditUserAsync(userEdit, password);
            if (!result.Succeeded)
            {
                return result;
            }
            var img = _upload.ResizeImage(signature, 500, 500);
            var fileId = await _upload.Upload(img);
            if (!string.IsNullOrWhiteSpace(userEdit.Signature))
            {
                _upload.DeleteFile(userEdit.Signature);
            }

            userEdit.Signature = fileId;
            result = await _userManager.UpdateAsync(userEdit);

            if (result.Succeeded)
            {
                _userUow.Complete();
            }

            return result;
        }

        public async Task<IdentityResult> EditUserAsync(User userEdit, string password)
        {
            IdentityResult result = await _EditUserAsync(userEdit, password);

            if (result.Succeeded)
            {
                _userUow.Complete();
            }

            return result;
        }

        private async Task<IdentityResult> _EditUserAsync(User userEdit, string password)
        {
            _userUow.User.ClearCollection(userEdit, x => x.Units);
            IdentityResult result = await _userManager.UpdateAsync(userEdit);
            if (!result.Succeeded)
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                string passwordToken = await _userManager.GeneratePasswordResetTokenAsync(userEdit);
                result = await _userManager.ResetPasswordAsync(userEdit, passwordToken, password);
            }

            return result;
        }

        public async Task<IdentityResult> ChangeUserRolesAsync(User user, IList<string> roles)
        {
            //Safegaurd preventing powerAdmin change
            if (roles.Contains(Roles.PowerAdmin))
            {
                throw new UnauthorizedException("Cannot change to PowerAdmin.");
            }

            var checkResult = await _authService.VerifyRoles(roles);
            if (!checkResult.Succeeded)
            {
                return checkResult;
            }
            //get current roles
            var oldRoles = await _userManager.GetRolesAsync(user);
            //roles to remove
            IEnumerable<string> exceptRoles = oldRoles.Except(roles);
            //roles to add
            IEnumerable<string> addRoles = roles.Except(oldRoles);

            //remove old roles
            user.Units = null;
            var removeResult = await _userManager.RemoveFromRolesAsync(user, exceptRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }
            //add new roles
            var result = await _userManager.AddToRolesAsync(user, addRoles);

            if (result.Succeeded)
            {
                _userUow.Complete();
            }

            return result;
        }

        public async Task<UserResult> GetUserAsync(Guid id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return null;
            }
            user.Units = _userUow.User.GetUserUnitMap().Where(x => x.UserId == id).ToList();

            var roles = _userUow.Role.GetUserRoles(user.Id).ToList();
            return new UserResult
            {
                User = user,
                Roles = roles
            };
        }

        public async Task<IdentityResult> DeleteUser(Guid id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return null;
            }

            var result = await _userManager.DeleteAsync(user);

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            _userUow.User.ClearCollection(user, x => x.Units);
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }
    }
}
