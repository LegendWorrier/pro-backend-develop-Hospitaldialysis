using System;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class PrescriptionNurseRequest : RequestApprove
    {
        public const string KEY = "presc-nurse";
        public PrescriptionNurseRequest()
        {
            Type = KEY;
        }

        public Guid PrescriptionId { get; set; }
    }
}
