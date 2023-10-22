using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditExecutionRecordViewModel
    {
        public ExecutionType Type { get; set; }

        // ============ Medicine Execute ==================
        public UsageWays? OverrideRoute { get; set; }
        public float? OverrideDose { get; set; }


    }
}
