using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Wasenshi.HemoDialysisPro.Utils
{
    public static class Extension
    {
        public static IEnumerable<int> GetUnitList(this ClaimsPrincipal user)
        {
            return user.FindAll("unit").Select(x => int.Parse(x.Value));
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static Guid GetUserIdAsGuid(this ClaimsPrincipal user)
        {
            string userId = user.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Guid.Empty;
            }
            return Guid.Parse(userId);
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string ToSnakeCase(this string s)
        {
            return Regex.Replace(s, "[A-Z]", "_$0").TrimStart('_').ToLower();
        }

        public static string Capitalize(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var splits = text.Split(' ');
            int index = 0;
            foreach (var s in splits)
            {
                if (s.Length < 2)
                {
                    continue;
                }
                splits[index++] = char.ToUpper(s[0]) + s[1..];
            }

            return string.Join(' ', splits);
        }

        /// <summary>
        /// convert from unix timestamp in millisecond
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddMilliseconds(unixTimeStamp);
        }

        /// <summary>
        /// Cut-off the time part and represent this as UTC kind datetime.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime AsDate(this DateTimeOffset date)
        {
            return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        }

        /// <summary>
        /// Repersent this as UTC kind datetime (no conversion)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime AsDateTime(this DateTimeOffset date)
        {
            return DateTime.SpecifyKind(date.DateTime, DateTimeKind.Utc);
        }

        /// <summary>
        /// Convert to UTC date with time truncated. (start of day before conversion)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToUtcDate(this DateTimeOffset date)
        {
            var offsetTicks = date.Offset.Ticks;
            return date.AsDate().AddTicks(-offsetTicks);
        }

        /// <summary>
        /// Specify the datetime as UTC.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime AsUtcDate(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }

    public static class DateTimeDayOfMonthExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }
        public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, 1, 0, 0, 0, value.Offset);
        }

        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }
        public static DateTimeOffset LastDayOfMonth(this DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, value.Date.DaysInMonth(), 0, 0, 0, value.Offset);
        }
    }
}
