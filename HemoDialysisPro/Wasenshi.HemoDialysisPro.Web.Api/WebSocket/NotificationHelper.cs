using Microsoft.AspNetCore.Mvc.Localization;
using RediSearchClient;
using RediSearchClient.Indexes;
using RediSearchClient.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public static class NotificationHelper
    {
        public static async Task CreateNotificationIndex(this IRedisPool redisPool)
        {
            var db = await redisPool.GetDatabaseAsync();

            if (db.ListIndexes().Any(x => x == "noti"))
            {
                return;
            }
            await db.CreateIndexAsync("noti",
                RediSearchIndex
                    .OnHash()
                    .ForKeysWithPrefix("urn:notification")
                    .WithSchema(
                        x => x.Text("Title"),
                        x => x.Text("Detail"),
                        x => x.Numeric("Type", true),
                        x => x.Tag("Users"),
                        x => x.Tag("Roles"),
                        x => x.Tag("Units"),
                        x => x.Numeric("IsGlobal"),
                        x => x.Numeric("Created", true),
                        x => x.Numeric("Remove", true)
                    )
                    .Build()
                );
        }

        public static async Task<(IEnumerable<Notification> items, int total)> GetNotifications(this IRedisPool redisPool, IUser user, IEnumerable<string> roles, int max = 5, int page = 1)
        {
            var db = await redisPool.GetDatabaseAsync();
            string queryString = GetMainQuery(user, roles);
            var query = RediSearchQuery
                .On("noti")
                .UsingQuery(queryString)
                .SortBy("Created", Direction.Descending)
                .Limit((page - 1) * max, max)
                .Build();

            var result = db.Search(query);

            return result.AsNotifications();
        }

        public static async Task<(DateTime oldestRemoveDate, DateTime upperLimit, int count)> GetOldestNotiCount(this IRedisPool redisPool, IUser user, IEnumerable<string> roles, TimeZoneInfo timezone)
        {
            var db = await redisPool.GetDatabaseAsync();
            string queryString = GetMainQuery(user, roles);
            var query = RediSearchQuery.On("noti").UsingQuery(queryString).SortBy("Remove", Direction.Ascending);

            var check = db.Search(query.Limit(0, 1).Build()).FirstOrDefault();
            if (check == null)
            {
                return (DateTime.MinValue, DateTime.MinValue, 0);
            }
            DateTime target = ((double)check.Fields["Remove"]).FromSecondToDate();
            DateTimeOffset targetTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(target, TimeSpan.Zero), timezone);
            targetTz = targetTz.AddTicks(-targetTz.TimeOfDay.Ticks);
            double upperLimit = targetTz.AddDays(1).ToSeconds();

            var result = db.Search(query.WithNumericFilters(x => x.Field("Remove", 0, upperLimit, false, true)).Build());

            return (target, targetTz.AddDays(1).UtcDateTime, result.RecordCount);
        }

        public static (IEnumerable<Notification> items, int total) AsNotifications(this SearchResult searchResult)
        {
            return (searchResult.Select(x => new Notification
            {
                Id = new Guid(x.Fields["Id"].ToString()),
                Action = x.Fields["Action"].ToString().Split(','),
                Title = x.Fields["Title"].ToString(),
                Detail = x.Fields["Detail"].ToString(),
                Type = (NotificationType)(int)x.Fields["Type"],
                Created = ((double)x.Fields["Created"]).FromSecondToDate(),
                ExpireDate = ((double)x.Fields["Expire"]).FromSecondToDate(),
                Tags = x.Fields["Tags"].ToString().Split(',')
            }), searchResult.RecordCount);
        }

        public static NotificationInfo GetNotiInfo(this Notification notification, IHtmlLocalizer<Notification> localizer, string cultureKey)
        {
            var notiInfo = new NotificationInfo
            {
                Notification = notification,
                Languages = new Dictionary<string, string>()
            };
            var titleValues = notification.Title.Split("::");
            var detailValues = notification.Detail.Split("::");
            // the arg string would contain '{text}' if arg format is required
            var formatArgs = titleValues.Skip(1).Where(x => x.Contains("{")).Concat(detailValues.Skip(1).Where(x => x.Contains("{"))).ToHashSet();

            // default lang
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            notiInfo.Languages.Add("title_en", localizer[titleValues[0]].Value);
            notiInfo.Languages.Add("detail_en", localizer[detailValues[0]].Value);
            foreach (var item in formatArgs)
            {
                var key = item[1..^1];
                notiInfo.Languages.Add($"{key}_en", localizer[key].Value);
            }
            // culture lang
            var culture = CultureInfo.GetCultureInfo(cultureKey);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            notiInfo.Languages.Add($"title_{cultureKey}", localizer[titleValues[0]].Value);
            notiInfo.Languages.Add($"detail_{cultureKey}", localizer[detailValues[0]].Value);
            foreach (var item in formatArgs)
            {
                var key = item[1..^1];
                notiInfo.Languages.Add($"{key}_{cultureKey}", localizer[key].Value);
            }

            return notiInfo;
        }

        public static Notification ReplaceText(this Notification notification, IHtmlLocalizer<Notification> localizer, CultureInfo culture = null)
        {
            if (culture != null)
            {
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            else
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }

            var titleValues = notification.Title.Split("::");
            var detailValues = notification.Detail.Split("::");
            notification.Title = _FormatArg(string.Format(localizer[titleValues[0]].Value, titleValues.Skip(1).ToArray()), localizer, true);
            notification.Detail = _FormatArg(string.Format(localizer[detailValues[0]].Value, detailValues.Skip(1).ToArray()), localizer);

            return notification;
        }

        // =========== Util =================

        private static string GetMainQuery(IUser user, IEnumerable<string> roles)
        {
            string queryString;
            if (user.Units.Count == 0) // root admin case
            {
                queryString =
                    $"(@Users:{{ {user.Id.ToString().Replace("-", "\\-")} }})" +
                    $"| (@Roles:{{ all | root }})" +
                    $"| (@Units:{{ all | root }})" +
                    $"| (@IsGlobal:[(0 1])";
            }
            else
            {
                var units = string.Join("|", user.Units.Select(x => x.UnitId.ToString().Replace("-", "\\-")));
                queryString =
                    $"(@Users:{{ {user.Id.ToString().Replace("-", "\\-")} }})" +
                    $"| (@Roles:{{ {string.Join("|", roles)} }} @Units:{{ {units} }})" +
                    $"| (@Roles:{{ all }} @Units:{{ {units} }})" +
                    $"| (@Roles:{{ {string.Join("|", roles)} }} @Units:{{ all }})" +
                    $"| (@IsGlobal:[(0 1])";
            }

            return queryString;
        }

        private static string _FormatArg(string argString, IHtmlLocalizer<Notification> localizer, bool isTitle = false)
        {
            var start = argString.IndexOf("{");
            while (start > -1)
            {
                var end = argString.IndexOf("}", start);
                var targetStr = argString.Substring(start, end - start + 1);
                var replaced = localizer[targetStr[1..^1]].Value;
                if (isTitle)
                {
                    replaced = char.ToUpper(replaced[0]) + replaced[1..];
                }
                argString = argString.Replace(targetStr, replaced);
                start = argString.IndexOf("{", start);
            }
            return argString;
        }
    }
}
