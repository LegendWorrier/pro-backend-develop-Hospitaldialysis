using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Core
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IContextAdapter context;
        private readonly IUserAdapter userAdapter;

        public UserInfoService(IContextAdapter context, IUserAdapter userAdapter)
        {
            this.context = context;
            this.userAdapter = userAdapter;
        }

        public IEnumerable<UserResult> GetAllUsers(Expression<Func<IUser, bool>> condition = null)
        {
            IQueryable<IUser> allUsers = context.GetUsers();
            if (condition != null)
            {
                allUsers = allUsers.Where(condition);
            }
            var users = allUsers.ToList();

            var result = users.Select(u => new UserResult
            {
                User = u,
                Roles = userAdapter.GetUserRoles(u.Id).ToList()
            });

            return result;
        }

        public IEnumerable<IUser> GetDoctorList(int[] unitId)
        {
            var allUsers = context.GetUsers();
            var rolesMap = context.GetUserRoles();
            var roles = context.GetRoles();

            bool bypassUnit = unitId == null;

            IEnumerable<IUser> result = allUsers.Join(rolesMap, x => x.Id, y => y.UserId, (user, map) => new { User = user, map.RoleId })
                .Join(roles, x => x.RoleId, y => y.Id, (r, role) => new { User = r.User, Role = role.NormalizedName })
                .Where(x => x.Role == Roles.Doctor.ToUpper())
                .Where(x => bypassUnit || x.User.Units.Any(u => unitId.Contains(u.UnitId)))
                .Where(x => x.User.NormalizedUserName != "ROOTADMIN")
                .Select(x => x.User)
                .ToList();

            return result;
        }

        public IEnumerable<UserResult> GetNurseList(int[] unitId = null)
        {
            var allUsers = context.GetUsers();
            var rolesMap = context.GetUserRoles();
            var roles = context.GetRoles();

            bool bypassUnit = unitId == null || unitId.Length == 0;

            var initQuery = allUsers.Join(rolesMap, x => x.Id, y => y.UserId, (user, map) => new { User = user, map.RoleId })
                .Join(roles, x => x.RoleId, y => y.Id, (r, role) => new { r.User, Role = role.NormalizedName, RoleName = role.Name })
                .Where(x => x.Role == Roles.PN.ToUpper() || x.Role == Roles.Nurse.ToUpper() || x.Role == Roles.HeadNurse.ToUpper())
                .Where(x => x.User.NormalizedUserName != "ROOTADMIN");

            if (!bypassUnit)
            {
                initQuery = initQuery.Where(x => x.User.Units.Any(u => unitId.Contains(u.UnitId)));
            }

            IEnumerable<UserResult> result = initQuery
                .OrderBy(x => x.Role)
                .ThenBy(x => x.User.FirstName)
                .ThenBy(x => x.User.UserName)
                .Select(x => new UserResult() { User = x.User, Roles = new[] { x.RoleName } })
                .ToList();

            return result;
        }

        public UserResult FindUser(Expression<Func<IUser, bool>> expression)
        {
            IUser user = context.GetUsers().FirstOrDefault(expression);
            if (user == null)
            {
                return null;
            }

            var roles = userAdapter.GetUserRoles(user.Id).ToList();

            return new UserResult
            {
                User = user,
                Roles = roles
            };
        }

        public IEnumerable<int> GetUserUnits(Guid userId)
        {
            return userAdapter.GetUserUnitMap().Where(x => x.UserId == userId).Select(x => x.UnitId);
        }
    }
}
