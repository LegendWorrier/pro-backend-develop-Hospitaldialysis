using Microsoft.AspNetCore.SignalR;
using Serilog;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public class CheckInHub : Hub
    {
        private readonly MonitorPool monitorPool;
        private readonly IHubContext<HemoBoxHub, IHemoBoxClient> hemobox;
        private readonly IPatientService patientService;
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;

        public CheckInHub(
            IHubContext<HemoBoxHub, IHemoBoxClient> hemobox,
            IPatientService patientService,
            IHemoService hemoService,
            IRecordService recordService,
            IRedisClient redis,
            IMessageQueueClient message
            )
        {
            this.monitorPool = redis.GetMonitorPool();
            this.hemobox = hemobox;
            this.patientService = patientService;
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.redis = redis;
            this.message = message;
        }

        /// <summary>
        /// This method is called when patient card is touched on Check-In station, or manually find patient is executed.
        /// The patient name will be on the screen. We will provide live status about pre/post weight of this patient.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PatientInfo> FindPatient(PatientReq req)
        {
            var patient = req.Rfid ? patientService.GetPatientByRFID(req.Id) : patientService.GetPatient(req.Id);
            if (patient == null)
            {
                return null;
            }

            float weight = 0;
            bool postWeight = false;
            var hemosheet = hemoService.GetHemodialysisRecordByPatientId(patient.Id);
            if (hemosheet != null)
            {
                weight = hemosheet.Dehydration.PreTotalWeight;
                postWeight = hemosheet.IsPostWeight(recordService);
            }

            // signal live status tracking
            if (!postWeight)
            {
                var connectionId = Context.ConnectionId;
                var hemoId = hemosheet?.Id;

                message.Publish(new CheckInPostWeight
                {
                    ConnectionId = connectionId,
                    HemoId = hemoId,
                    PatientId = patient.Id,
                });
            }

            return new PatientInfo
            {
                Id = patient.Id,
                Name = patient.Name,
                Rfid = patient.RFID ?? "",
                Weight = weight,
                PostWeight = postWeight,
            };
        }

        public async Task<object> CheckIn(CheckInData req)
        {
            Patient patient = patientService.GetPatient(req.Id);
            if (patient == null)
            {
                return null;
            }
            HemodialysisRecord hemosheet = hemoService.GetHemodialysisRecordByPatientId(patient.Id);
            bool postWeight = false;
            if (hemosheet == null)
            {
                var shiftInfo = redis.GetUnitShift(patient.UnitId);
                var record = new HemodialysisRecord
                {
                    PatientId = patient.Id,
                    Dehydration = new DehydrationRecord
                    {
                        CheckInTime = DateTime.UtcNow
                    },

                    IsSystemUpdate = true
                };
                if (req.IsTare)
                {
                    record.Dehydration.WheelchairWeight = req.Weight;
                    // default to the same value
                    record.Dehydration.PostWheelchairWeight = req.Weight;
                }
                else
                {
                    record.Dehydration.PreTotalWeight = req.Weight;
                }

                record = hemoService.CreateHemodialysisRecord(record, shiftInfo?.CurrentSection);

                if (FeatureFlag.HasIntegrated())
                {
                    var target = NotificationTarget.ForNurses(patient.UnitId);
                    var notification = redis.AddNotification(
                        "CheckIn_title",
                        $"CheckIn_detail::{patient.Name}",
                        new[] { "page", "patient", patient.Id },
                        target,
                        new[] { "checkin" }
                        );
                    message.Publish(notification);
                }
            }
            else
            {
                if (!req.IsPre && hemosheet.IsPostWeight(recordService))
                {
                    if (req.IsTare)
                    {
                        hemosheet.Dehydration.PostWheelchairWeight = req.Weight;
                    }
                    else
                    {
                        hemosheet.Dehydration.PostTotalWeight = req.Weight;
                    }
                    postWeight = true;
                }
                else
                {
                    if (req.IsTare)
                    {
                        hemosheet.Dehydration.WheelchairWeight = req.Weight;
                    }
                    else
                    {
                        hemosheet.Dehydration.PreTotalWeight = req.Weight;
                    }
                }

                hemosheet.IsSystemUpdate = true;
                hemoService.EditHemodialysisRecord(hemosheet);
            }

            var result = new
            {
                Id = patient.Id,
                Name = patient.Name,
                Rfid = patient.RFID,
                PostWeight = postWeight
            };

            return result;
        }

        public override Task OnConnectedAsync()
        {
            Log.Information($"Check In station connected. [{Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            Log.Information($"Check In station disconnected. [{Context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }

    }

    public static class HemodialysisRecordExtensions
    {
        public static bool IsPostWeight(this HemodialysisRecord hemosheet, IRecordService recordService)
        {
            bool hasRecord = recordService.CheckDialysisRecordExistByHemoId(hemosheet.Id);
            var elaspedTime = DateTime.UtcNow - (hemosheet.Dehydration.CheckInTime ?? DateTime.UtcNow);
            return hasRecord || elaspedTime >= TimeSpan.FromHours(1);
        }
    }
}
