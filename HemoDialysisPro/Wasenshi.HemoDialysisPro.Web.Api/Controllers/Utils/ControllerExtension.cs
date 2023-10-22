using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils
{
    public static class ControllerExtension
    {
        public static void ValidateUnitHeadOrInCharged(this ControllerBase controller, int unitId, IMasterDataService masterDataService, IShiftService shiftService, IRedisClient redis)
        {
            // in case user is already head nurse or admin or doctor, skip this check and auto pass.
            if (controller.User.IsInRole(Roles.HeadNurse) || controller.User.IsInRole(Roles.Doctor) || controller.User.IsInRole(Roles.Admin))
            {
                return;
            }

            if (!CheckUnitHeadOrInCharged(controller, unitId, masterDataService, shiftService, redis))
            {
                throw new UnauthorizedException("No permission for this unit.");
            }
        }

        public static bool CheckUnitHeadOrInCharged(this ControllerBase controller, int unitId, IMasterDataService masterDataService, IShiftService shiftService, IRedisClient redis)
        {
            var userId = new Guid(controller.User.GetUserId());

            var isUnitHead = masterDataService.IsUnitHead(userId, unitId);
            if (isUnitHead)
            {
                return true;
            }

            var isIncharge = shiftService.IsIncharge(userId, unitId, redis.GetUnitShift(unitId).CurrentSection);

            return isIncharge;
        }

        public static void SetSessionForPatients(this IEnumerable<PatientViewModel> data, IRedisClient redis)
        {
            // set sessions
            foreach (var item in data)
            {
                item.IsInSession = redis.IsInSession(item.Id);
            }
        }
    }
}
