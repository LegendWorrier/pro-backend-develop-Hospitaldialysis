using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class DialysisPrescriptionViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }

        public bool Temporary { get; set; }
        public string Mode { get; set; }

        // ======== HDF Additional Info ===========
        public string HdfType { get; set; }
        public float? SubstituteVolume { get; set; }
        public float? IvSupplementVolume { get; set; }
        public string IvSupplementPosition { get; set; }

        // ----------------------------------------

        public float? DryWeight { get; set; }
        public float? ExcessFluidRemovalAmount { get; set; }
        public float? BloodFlow { get; set; }
        public int Duration { get; set; } // Total minutes
        public short? Frequency { get; set; }

        public string Anticoagulant { get; set; } // use masterdata in FE part
        public float? AcPerSession { get; set; } // unit/session
        public float? InitialAmount { get; set; } // unit
        public float? MaintainAmount { get; set; } // unit/Hr
        public string ReasonForRefraining { get; set; }

        // support ml unit for heparin
        public float? AcPerSessionMl { get; set; } // ml/session
        public float? InitialAmountMl { get; set; } // ml
        public float? MaintainAmountMl { get; set; } // ml/Hr

        // ==== Use masterdata in FE part ===
        public float? DialysateK { get; set; }

        public float? DialysateCa { get; set; }

        // -----------------------------------
        public float? HCO3 { get; set; }
        public float? Na { get; set; }
        public float? DialysateTemperature { get; set; }
        public float? DialysateFlowRate { get; set; }

        public string BloodAccessRoute { get; set; }
        public float? ANeedleCC { get; set; }
        public float? VNeedleCC { get; set; }
        public int? ArterialNeedle { get; set; }
        public int? VenousNeedle { get; set; }
        public string Dialyzer { get; set; }
        public float? DialyzerSurfaceArea { get; set; }

        public float? BloodTransfusion { get; set; }
        public float? ExtraFluid { get; set; }

        public float? AvgDialyzerReuse { get; set; } // For TRT Mapping

        public Guid? DialysisNurse { get; set; }

        public string Note { get; set; }

        // ========== For FE =============
        public bool IsHistory { get; set; }
    }
}