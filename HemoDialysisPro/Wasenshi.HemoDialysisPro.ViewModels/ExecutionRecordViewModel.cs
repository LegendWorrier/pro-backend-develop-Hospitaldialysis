using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ExecutionRecordViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public Guid HemodialysisId { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public ExecutionType Type { get; set; }

        public bool IsExecuted { get; set; }
        public Guid? CoSign { get; set; }


        public Guid? RecordId { get; set; } // flush NSS
        public Guid? PrescriptionId { get; set; } // medicine execute
        public UsageWays? OverrideRoute { get; set; } // medicine execute
        public float? OverrideDose { get; set; } // medicine execute
        public MedicinePrescriptionViewModel Prescription { get; set; }
    }
}
