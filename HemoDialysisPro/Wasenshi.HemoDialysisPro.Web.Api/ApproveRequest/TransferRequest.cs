using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class TransferRequest : RequestApprove
    {
        public const string KEY = "transfer";
        public TransferRequest()
        {
            Type = KEY;
        }

        public string PatientId { get; set; }
        public int SectionId { get; set; }
        public SectionSlots Slot { get; set; }
        public TransferTarget Target { get; set; }

        public class TransferTarget
        {
            public int UnitId { get; set; }
            public int SectionId { get; set; }
            public SectionSlots Slot { get; set; }
        }
    }
}
