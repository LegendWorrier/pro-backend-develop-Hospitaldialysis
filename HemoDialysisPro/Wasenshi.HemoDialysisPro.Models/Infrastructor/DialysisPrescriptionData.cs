using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class DialysisPrescriptionData : EntityBase<Guid>
    {
        public bool Temporary { get; set; }
        [Required]
        [JsonConverter(typeof(JsonEnumConverter<DialysisMode>))]
        public DialysisMode Mode { get; set; }

        // ======== HDF Additional Info ===========
        [JsonConverter(typeof(JsonEnumConverter<HdfType?>))]
        public HdfType? HdfType { get; set; }
        public float? SubstituteVolume { get; set; }
        public float? IvSupplementVolume { get; set; }
        public string IvSupplementPosition { get; set; }

        // ----------------------------------------

        public float? DryWeight { get; set; }
        public float? ExcessFluidRemovalAmount { get; set; }
        public float? BloodFlow { get; set; }
        public float? BloodTransfusion { get; set; } // ml
        public float? ExtraFluid { get; set; } // ml

        [Required]
        public TimeSpan Duration { get; set; }
        [Range(1, 7)]
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

        public string BloodAccessRoute { get; set; } // use built-in masterdata in FE part
        // ========== Ac For Fill ==============
        public float? ANeedleCC { get; set; }
        public float? VNeedleCC { get; set; }
        // ==== Use masterdata in FE part ===
        public int? ArterialNeedle { get; set; }
        public int? VenousNeedle { get; set; }
        // -----------------------------------
        public string Dialyzer { get; set; }  // use masterdata in FE part
        public float? DialyzerSurfaceArea { get; set; }

        //-------------------------------

        public float? AvgDialyzerReuse { get; set; } // For TRT Mapping

        public Guid? DialysisNurse { get; set; }

        public string Note { get; set; }
    }
}
