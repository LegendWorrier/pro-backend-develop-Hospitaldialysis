using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils
{
    [Authorize(Roles = Roles.PowerAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IRedisPool pool;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly IUserInfoService userInfoService;

        public NotificationController(
            IRedisPool pool,
            IRedisClient redis,
            IMessageQueueClient message,
            IUserInfoService userInfoService)
        {
            this.pool = pool;
            this.redis = redis;
            this.message = message;
            this.userInfoService = userInfoService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetNotification()
        {
            var result = await _GetNotification(new Guid(User.GetUserId()));
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetNotification(Guid userId)
        {
            var result = await _GetNotification(userId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        private async Task<NotificationResult> _GetNotification(Guid userId)
        {
            var user = userInfoService.FindUser(x => x.Id == userId);
            if (user == null)
            {
                return null;
            }
            var notifications = await pool.GetNotifications(user.User, user.Roles);

            return new NotificationResult
            {
                Data = notifications.items,
                Total = notifications.total
            };
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreateNotification([FromBody] NotificationInput notification)
        {
            var targetExp = notification.TargetExpression;
            NotificationTarget target;
            var args = targetExp.Split(":").Last();
            if (targetExp == "all")
            {
                target = NotificationTarget.Global();
            }
            else if (targetExp.StartsWith("unit:"))
            {
                target = NotificationTarget.ForUnits(targetExp.Replace("unit:", "").Split(',').Select(x => int.Parse(x)));
            }
            else if (targetExp.StartsWith("user:"))
            {
                target = NotificationTarget.ForUser(targetExp.Replace("user:", "").Split(',').Select(x => new Guid(x)).ToArray());
            }
            else if (targetExp.StartsWith("nurse"))
            {
                target = NotificationTarget.ForNurses(string.IsNullOrWhiteSpace(args.Replace("nurse", "")) ? null : args.Split(',').Select(x => int.Parse(x)).ToArray());
            }
            else if (targetExp.StartsWith("doctor"))
            {
                target = NotificationTarget.ForDoctors(string.IsNullOrWhiteSpace(args.Replace("doctor", "")) ? null : args.Split(',').Select(x => int.Parse(x)).ToArray());
            }
            else if (targetExp.StartsWith("head"))
            {
                target = NotificationTarget.ForHeadNurses(string.IsNullOrWhiteSpace(args.Replace("head", "")) ? null : args.Split(',').Select(x => int.Parse(x)).ToArray());
            }
            else if (targetExp.StartsWith("admin"))
            {
                target = NotificationTarget.ForAdmin(string.IsNullOrWhiteSpace(args.Replace("admin", "")) ? null : args.Split(',').Select(x => int.Parse(x)).ToArray());
            }
            else
            {
                if (string.IsNullOrWhiteSpace(targetExp))
                {
                    target = NotificationTarget.ForUser(new Guid(User.GetUserId()));
                }
                else
                {
                    target = NotificationTarget.ForUser(targetExp.Split(',').Select(x => new Guid(x)).ToArray());
                }
            }

            var result = redis.AddNotification(notification.Title, notification.Detail, notification.Action, notification.Type, target,
                notification.Expire.HasValue ? notification.Expire.Value.UtcDateTime : DateTime.UtcNow.AddDays(1), notification.Tags);
            message.SendNotificationEvent(result, target);

            return Ok(result);
        }

        public class NotificationInput
        {
            public string Title { get; set; }
            public string Detail { get; set; }
            public string[] Action { get; set; }
            public NotificationType Type { get; set; } = NotificationType.Info;

            public string TargetExpression { get; set; }

            public DateTimeOffset? Expire { get; set; }
            public string[] Tags { get; set; }
        }
    }
}
