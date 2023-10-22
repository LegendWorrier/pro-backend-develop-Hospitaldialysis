using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using AppRoles = Wasenshi.HemoDialysisPro.Constants.Roles;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class RedisNotificationExtension
    {
        public static Notification AddNotification(this IRedisClient redis, string title, string detail, string[] action, NotificationTarget target, params string[] tags)
        {
            return AddNotification(redis, title, detail, action, NotificationType.Info, target, DateTime.UtcNow.AddDays(3), tags);
        }

        public static Notification AddNotification(this IRedisClient redis, string title, string detail, string[] action, NotificationType type, NotificationTarget target, params string[] tags)
        {
            return AddNotification(redis, title, detail, action, type, target, DateTime.UtcNow.AddDays(3), tags);
        }

        public static Notification AddNotification(this IRedisClient redis, string title, string detail, string[] action, NotificationTarget target, DateTime expireDate, params string[] tags)
        {
            return AddNotification(redis, title, detail, action, NotificationType.Info, target, expireDate, tags);
        }

        public static Notification AddNotification(this IRedisClient redis, string title, string detail, string[] action, NotificationType type, NotificationTarget target, DateTime expireDate, params string[] tags)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Action = action,
                Title = title,
                Detail = detail,
                Type = type,
                ExpireDate = expireDate,
                Created = DateTime.UtcNow,
                Tags = tags
            };

            var hashes = redis.Hashes[$"urn:notification:{notification.Id}"];

            hashes.Add(nameof(notification.Id), notification.Id.ToString());
            hashes.Add(nameof(notification.Title), notification.Title);
            hashes.Add(nameof(notification.Detail), notification.Detail);
            hashes.Add(nameof(notification.Action), string.Join(",", notification.Action));
            hashes.Add(nameof(notification.Type), ((int)notification.Type).ToString());
            hashes.Add(nameof(notification.Created), notification.Created.ToSeconds().ToString());
            hashes.Add("Expire", notification.ExpireDate.ToSeconds().ToString());
            HashSet<string> tagsSet = new HashSet<string>(notification.Tags);
            hashes.Add(nameof(notification.Tags), string.Join(",", tagsSet));

            var units = target.Units?.Select(x => x.ToString());
            if (target.IsGlobal)
            {
                hashes.Add("IsGlobal", "1");
            }
            else if (target.IsForUser)
            {
                hashes.Add("Users", string.Join(",", target.Users.Select(x => x.ToString())));
            }
            else if (target.IsForUnit)
            {
                hashes.Add("Units", string.Join(",", units) + ", root");
            }
            else
            {
                if (!target.Units?.Any() ?? true)
                {
                    hashes.Add("Units", "all");
                }
                else
                {
                    hashes.Add("Units", string.Join(",", units) + ", root");
                }

                if (!target.Roles?.Any() ?? true)
                {
                    hashes.Add("Roles", "all");
                }
                else
                {
                    hashes.Add("Roles", string.Join(",", target.Roles) + ", root");
                }

                // with user
                if (target.Users?.Any() ?? false)
                {
                    hashes.Add("Users", string.Join(",", target.Users.Select(x => x.ToString())));
                }
            }

            DateTime targetExpireTime = (notification.Created - expireDate).Days < 7 ? notification.Created.AddDays(7).UtcDateTime : expireDate;
            hashes.Add("Remove", targetExpireTime.ToSeconds().ToString());

            redis.ExpireEntryAt($"urn:notification:{notification.Id}", targetExpireTime);
            redis.Save();

            return notification;
        }

        public static void SendNotificationEvent(this IMessageQueueClient message, Notification notification, NotificationTarget target)
        {
            message.Publish(new NotificationEvent
            {
                NotificationId = notification.Id,
                Target = target
            });
        }

        public static Notification GetNotification(this IRedisClient redis, Guid id)
        {
            if (!redis.ContainsKey($"urn:notification:{id}"))
            {
                return null;
            }

            var hashes = redis.Hashes[$"urn:notification:{id}"];
            var result = new Notification
            {
                Id = id,
                Action = hashes["Action"].Split(','),
                Title = hashes["Title"],
                Detail = hashes["Detail"],
                Type = (NotificationType)int.Parse(hashes["Type"]),
                Created = double.Parse(hashes["Created"]).FromSecondToDate(),
                ExpireDate = double.Parse(hashes["Expire"]).FromSecondToDate(),
                Tags = hashes["Tags"].Split(',')
            };

            return result;
        }

        public static void ApproveNotification(this IRedisClient redis, Notification notification)
        {
            ApproveNotification(redis, notification.Id);
        }

        public static void DenyNotification(this IRedisClient redis, Notification notification)
        {
            DenyNotification(redis, notification.Id);
        }

        public static void ApproveNotification(this IRedisClient redis, Guid notificationId)
        {
            var hashes = redis.Hashes[$"urn:notification:{notificationId}"];
            HashSet<string> tags = new HashSet<string>(hashes["Tags"].Split(','));
            tags.Add("approved");

            hashes["Tags"] = string.Join(",", tags);
            redis.Save();
        }

        public static void DenyNotification(this IRedisClient redis, Guid notificationId)
        {
            var hashes = redis.Hashes[$"urn:notification:{notificationId}"];
            HashSet<string> tags = new HashSet<string>(hashes["Tags"].Split(','));
            tags.Add("denied");

            hashes["Tags"] = string.Join(",", tags);
            redis.Save();
        }

        public static void SetRequestNotiInvalid(this IRedisClient redis, Guid notificationId)
        {
            var hashes = redis.Hashes[$"urn:notification:{notificationId}"];
            HashSet<string> tags = new HashSet<string>(hashes["Tags"].Split(','));
            tags.Add("invalid");

            hashes["Tags"] = string.Join(",", tags);
            redis.Save();
        }
    }

    public class NotificationTarget
    {
        public IEnumerable<Guid> Users { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<int> Units { get; set; }

        public bool IsForUser { get; private set; } = false;
        public bool IsForUnit { get; private set; } = false;

        public bool IsGlobal { get; private set; } = false;

        private NotificationTarget() { }

        public static NotificationTarget ForUser(params Guid[] userIds)
        {
            return new NotificationTarget
            {
                Users = userIds,
                IsForUser = true
            };
        }

        public static NotificationTarget ForHeadNurses(params int[] targetUnits)
        {
            return new NotificationTarget
            {
                Roles = new[] { AppRoles.HeadNurse },
                Units = targetUnits
            };
        }

        public static NotificationTarget ForNurses(params int[] targetUnits)
        {
            return new NotificationTarget
            {
                Roles = new[] { AppRoles.Nurse, AppRoles.HeadNurse },
                Units = targetUnits
            };
        }

        public static NotificationTarget ForDoctors(params int[] targetUnits)
        {
            return new NotificationTarget
            {
                Roles = new[] { AppRoles.Doctor },
                Units = targetUnits
            };
        }

        public static NotificationTarget ForAdmin(params int[] targetUnits)
        {
            return new NotificationTarget
            {
                Roles = new[] { AppRoles.Admin },
                Units = targetUnits
            };
        }

        public static NotificationTarget ForUnit(int targetUnit, params int[] additionalUnits)
        {
            return ForUnits(new[] { targetUnit }.Concat(additionalUnits));
        }

        public static NotificationTarget ForUnits(IEnumerable<int> units)
        {
            return new NotificationTarget
            {
                Units = units,
                IsForUnit = true
            };
        }

        public static NotificationTarget Global()
        {
            return new NotificationTarget
            {
                IsGlobal = true
            };
        }
    }

    public static class NotificationTargetExtension
    {
        public static NotificationTarget WithUser(this NotificationTarget target, params Guid[] userIds)
        {
            target.Users = (target.Users ?? Array.Empty<Guid>()).Concat(userIds);
            return target;
        }
    }

    public class NotificationEvent
    {
        public Guid NotificationId { get; set; }
        public NotificationTarget Target { get; set; }
    }
}
