using ServiceStack.Redis;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class RedisHemoSessionManagementExtension
    {
        public static IRedisHash GetAllHemoSessions(this IRedisClient redis)
        {
            IRedisHash sessions = redis.Hashes[Common.SESSION_LIST];

            return sessions;
        }

        public static bool AddNewSession(this IRedisClient redis, HemodialysisRecord hemosheet)
        {
            IRedisHash sessions = redis.Hashes[Common.SESSION_LIST];
            bool success = sessions.TryAdd(Common.GetSessionKey(hemosheet.PatientId), Common.GetHemosheetKey(hemosheet.Id));

            return success;
        }

        public static bool RemoveSession(this IRedisClient redis, HemodialysisRecord hemosheet)
        {
            return redis.RemoveSession(hemosheet.PatientId);
        }

        public static bool RemoveSession(this IRedisClient redis, string patientId)
        {
            IRedisHash sessions = redis.Hashes[Common.SESSION_LIST];
            bool success = sessions.Remove(Common.GetSessionKey(patientId));

            return success;
        }

        public static bool IsInSession(this IRedisClient redis, string patientId)
        {
            IRedisHash sessions = redis.Hashes[Common.SESSION_LIST];
            return sessions.ContainsKey(Common.GetSessionKey(patientId));
        }

        public static string GetHemoKeyForCurrentSession(this IRedisClient redis, string patientId)
        {
            IRedisHash sessions = redis.Hashes[Common.SESSION_LIST];

            if (sessions.TryGetValue(Common.GetSessionKey(patientId), out var hemoId))
            {
                return hemoId;
            }

            return null;
        }
    }
}
