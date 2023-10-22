using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class CreateExecutionRecordViewModel : EntityViewModel
    {
        public Guid HemodialysisId { get; set; }
        public DateTime Timestamp { get; set; }

        public ExecutionType Type { get; set; }

        public bool IsExecuted { get; set; }

        public Guid? CoSign { get; set; }
    }
}
