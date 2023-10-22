using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Wasenshi.HemoDialysisPro.Models.TimezoneUtil
{
    /// <summary>
    /// This class is a helper for getting the right timezone for application.
    /// <br></br>
    /// <br></br>
    /// The id of each timezone is internally hard-coded based on the chosen image docker.
    /// </summary>
    public static class TimezoneUtils
    {
        public static readonly ImmutableDictionary<string, string> timezoneIdMapping = new Dictionary<string, string>
        {
            ["FR"] = "Europe/Paris",    // France
            ["IN"] = "Asia/Kolkata",    // India
            ["IL"] = "Asia/Jerusalem",  // Israel
            ["MM"] = "Asia/Yangon",     // Myanmar
            ["KH"] = "Asia/Phnom_Penh", // Khamen (Cambodia) กัมพูชา
            ["TH"] = "Asia/Bangkok",
            ["VN"] = "Asia/Ho_Chi_Minh",// Vietnam
            ["LA"] = "Asia/Vientiane",  // Lao
            ["JP"] = "Asia/Tokyo",
            ["KR"] = "Asia/Seoul",
            ["CN"] = "Asia/Shanghai",   // China
            ["TW"] = "Asia/Taipei",     // Taiwan
            ["HK"] = "Asia/Hong_Kong",  // Hong Kong
            ["PH"] = "Asia/Manila",     // Philippine
            ["ID"] = "Asia/Jakarta",    // Indonesia
            ["SG"] = "Asia/Singapore",  // Singapore
            ["AU"] = "Asia/Sydney"      // Australia
        }.ToImmutableDictionary();

        /// <summary>
        /// Input either country 2-digit code or timezone id
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZone(string code)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timezoneIdMapping.ContainsKey(code) ? timezoneIdMapping[code] : code);
            }
            catch (TimeZoneNotFoundException)
            {
                return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }
    }
}
