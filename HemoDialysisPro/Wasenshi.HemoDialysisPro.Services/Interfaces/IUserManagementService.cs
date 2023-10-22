using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IUserManagementService : IApplicationService
    {
        Task<UserResult> GetUserAsync(Guid id);
        Task<IdentityResult> EditUserAsync(User user, string password);
        Task<IdentityResult> EditUserAsync(User userEdit, string password, IFormFile signature);
        Task<IdentityResult> ChangeUserRolesAsync(User user, IList<string> roles);
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);
        Task<IdentityResult> DeleteUser(Guid id);
    }
}
