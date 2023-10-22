using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Core.Interfaces
{
    public interface IUserInfoService : IApplicationService
    {
        IEnumerable<UserResult> GetAllUsers(Expression<Func<IUser, bool>> condition = null);
        IEnumerable<IUser> GetDoctorList(int[] unitId);
        /// <summary>
        /// Distinguishable by role (Nurse, HeadNurse, PN), but unknown about admin status.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        IEnumerable<UserResult> GetNurseList(int[] unitId = null);
        IEnumerable<int> GetUserUnits(Guid userId);
        UserResult FindUser(Expression<Func<IUser, bool>> expression);
    }
}
