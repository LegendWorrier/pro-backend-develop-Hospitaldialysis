using ServiceStack.Redis;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class RedisShiftManagementExtension
    {
        public static IEnumerable<UnitShift> GetAllUnitShifts(this IRedisClient redis)
        {
            var unitShifts = redis.As<UnitShift>().GetAll();
            return unitShifts;
        }

        public static UnitShift GetUnitShift(this IRedisClient redis, int unitId)
        {
            var result = redis.As<UnitShift>().GetById(unitId);
            return result;
        }

        public static void UpdateUnitShift(this IRedisClient redis, int unitId, UnitShift data)
        {
            redis.As<UnitShift>().Store(data);
        }

        public static bool UnitExists(this IRedisClient redis, int unitId)
        {
            return redis.As<Unit>().GetById(unitId) != null;
        }
    }
}
