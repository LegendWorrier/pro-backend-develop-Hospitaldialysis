using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class TempTransferRequest : RequestApprove
    {
        public const string KEY = "transfer-temp";
        public TempTransferRequest()
        {
            Type = KEY;
        }

        public string PatientId { get; set; }
        public int SectionId { get; set; }
        public SectionSlots Slot { get; set; }
        public RescheduleViewModel Request { get; set; }
    }
}
