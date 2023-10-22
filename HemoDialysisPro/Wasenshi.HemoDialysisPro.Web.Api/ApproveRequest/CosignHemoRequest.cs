using System;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class CosignHemoRequest : RequestApprove
    {
        public const string KEY = "cosign-hemo";
        public CosignHemoRequest()
        {
            Type = KEY;
        }

        public Guid HemoId { get; set; }
    }
}
