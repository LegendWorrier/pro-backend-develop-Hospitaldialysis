using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ServiceStack.Redis;
using Wasenshi.HemoDialysisPro.Share;
using Microsoft.AspNetCore.SignalR;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;
using System.IO;
using Serilog;
using System.Threading;
using System.IO.Pipelines;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("box/log", Name = "hemobox")]
    public class HemoBoxLogController : Controller
    {
        private readonly IRedisClient redis;
        private readonly IHubContext<HemoBoxHub, IHemoBoxClient> hemobox;
        internal static readonly Dictionary<string, TaskCompletionSource<DelegateStream>> ResponseTasks = new Dictionary<string, TaskCompletionSource<DelegateStream>>();
        internal static readonly Dictionary<string, TaskCompletionSource> ConfirmTask = new Dictionary<string, TaskCompletionSource>();

        public HemoBoxLogController(IRedisClient redis, IHubContext<HemoBoxHub, IHemoBoxClient> hemobox)
        {
            this.redis = redis;
            this.hemobox = hemobox;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var moniter = redis.GetMonitorPool();
            var moniterList = moniter.BedList;
            ViewData["unitList"] = moniter.UnitListFromCache();
            return View("HemoBoxLog", moniterList);
        }

        [HttpGet("{macAddress}")]
        public async Task<IActionResult> GetLog(string macAddress, int logNo = 0)
        {
            if (ModelState.IsValid)
            {
                var moniter = redis.GetMonitorPool();
                var bed = moniter.GetBedByMacAddress(macAddress);
                var connectionId = bed?.ConnectionId;
                if (string.IsNullOrEmpty(connectionId))
                {
                    return NotFound();
                }
                if (!bed.Online)
                {
                    return BadRequest("Hemobox is offline.");
                }

                var secureKey = Guid.NewGuid().ToString();
                Log.Debug("Secure key is: {0}", secureKey);

                var response = new TaskCompletionSource<DelegateStream>();
                var confirm = new TaskCompletionSource();
                ResponseTasks.Add(secureKey, response);
                ConfirmTask.Add(secureKey, confirm);
                Timer timeout = new Timer((o) => response.TrySetCanceled(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

                bool hasError = false;
                try
                {
                    var box = hemobox.Clients.Client(connectionId);
                    _ = box.GetLog(secureKey, logNo);

                    DelegateStream input = await response.Task;
                    Response.ContentType = "application/octet-stream";
                    Response.ContentLength = input.Size;
                    Response.OnCompleted(async () =>
                    {
                        Log.Information("Finish log receiving. Cleansing up and closing all the request..");
                        ResponseTasks.Remove(secureKey);
                        confirm.SetResult();
                        ConfirmTask?.Remove(secureKey);
                    });

                    var filename = $"hemobox-{bed.MacAddress}-{(bed.Name ?? "noname")}.log{(logNo > 0 ? "." + logNo : "")}";

                    return File(input.ContentReader.AsStream(), "application/octet-stream", filename);
                }
                catch (TaskCanceledException)
                {
                    hasError = true;
                    Log.Information($"No responding from hemobox. [{connectionId}]");
                    throw new HubException($"TIMEOUT:No responding from hemobox. [{connectionId}]");
                }
                catch (Exception e)
                {
                    hasError = true;
                    Log.Error(e, "Failed to download hemobox log");
                }
                finally
                {
                    if (hasError)
                    {
                        ResponseTasks.Remove(secureKey);
                        confirm.SetResult();
                        ConfirmTask?.Remove(secureKey);
                    }
                }
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> UploadLog()
        {
            var request = HttpContext.Request;
            string secureKey = request.Headers["SecureKey"];
            if (string.IsNullOrWhiteSpace(secureKey))
            {
                return BadRequest("Invalid secure key.");
            }
            if (!ResponseTasks.TryGetValue(secureKey, out var response))
            {
                Log.Debug("Secure key from box is invalid: {0}", secureKey);
                return BadRequest("Invalid secure key.");
            }

            // delegate streaming to original user request
            var result = new DelegateStream
            {
                Content = request.Body,
                Size = request.ContentLength.Value,
                ContentReader = request.BodyReader
            };

            response.SetResult(result);

            if (ConfirmTask.TryGetValue(secureKey, out var confirm))
            {
                await confirm.Task;
            }
            else
            {
                Log.Error("No confirm task for hemobox to wait. Ending stream request...");
            }

            return Ok();
        }

        internal class DelegateStream
        {
            public Stream Content { get; set; }
            public long Size { get; set; }

            public PipeReader ContentReader { get; set; }

        }
    }
}
