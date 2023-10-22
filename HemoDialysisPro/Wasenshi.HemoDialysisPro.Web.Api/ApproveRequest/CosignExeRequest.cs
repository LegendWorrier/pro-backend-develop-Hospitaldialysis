using System;

namespace Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest
{
    public class CosignExeRequest : RequestApprove
    {
        public const string KEY = "cosign-execute";
        public CosignExeRequest()
        {
            Type = KEY;
        }

        public Guid ExecutionId { get; set; }
    }
}
