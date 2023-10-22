using System;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class RequestApprove
    {
        public RequestApprove()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid Requester { get; set; }
        public Guid? Approver { get; set; }
        public int? TargetUnitId { get; set; }
        public int? ExtraNotifyUnitId { get; set; }
        public string ExtraNotifyRole { get; set; }

        public string[] RequestArgs { get; set; }
        public Guid NotificationId { get; set; }

        public string Type { get; set; }
    }
}
