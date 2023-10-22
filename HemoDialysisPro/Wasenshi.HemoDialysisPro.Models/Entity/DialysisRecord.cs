using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class DialysisRecord : EntityBase<Guid>
    {
        [Required]
        public Guid HemodialysisId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public TimeSpan? Remaining { get; set; }
        // machine
        public string Model { get; set; }
        public string Number { get; set; }

        public int? BPS { get; set; }
        public int? BPD { get; set; }
        public int? HR { get; set; } // (/min)
        public int? RR { get; set; } // (/min)
        public float? Temp { get; set; } // Temperature (°C)
        public float? BFR { get; set; } // Blood Flow Rate (ml/min)
        public int? VP { get; set; } // Venous Pressure
        public int? AP { get; set; } // Arterial Pressure
        public int? DP { get; set; } // Dialysate Pressure
        public int? TMP { get; set; }
        public float? UFRate { get; set; } // (L/hr)
        public float? UFTotal { get; set; } // (L)
        public float? AcLoading { get; set; } // Heparin init (ml)
        public float? AcMaintain { get; set; } // Heparin rate (ml/hr)
        public float? HAV { get; set; } // Heparin Accumulated Volume (ml)
        public float? DFRTarget { get; set; } // Dialysate Flow Rate Target (ml/min)
        public float? DFR { get; set; } // Dialysate Flow Rate (ml/min)
        public string Dialysate { get; set; } // K/Ca
        public float? DTTarget { get; set; } // Dialysate Temperature target (°C)
        public float? DT { get; set; } // Dialysate Temperature (°C)
        public float? DCTarget { get; set; } // Dialysate Conductivity target (mS/cm)
        public float? DC { get; set; } // Dialysate Conductivity (mS/cm)
        public float? BC { get; set; } // Bicarb Conductivity (mS/cm)
        public float? NSS { get; set; } // Saline Solution Drip (ml)
        public float? Glucose50 { get; set; } // 50% w/v (ml)
        public float? HCO3 { get; set; }
        public float? NaTarget { get; set; } // (mmole)
        public string NaProfile { get; set; }
        public string UFProfile { get; set; }
        public string Mode { get; set; }
        public float? BFAV { get; set; } // Blood Flow Accumulated Volume (L)
        public float? UFTarget { get; set; } // UF goal (L)
        public float? SRate { get; set; } // Substitution Rate (L/hr)
        public float? SAV { get; set; } // Substitution Accumulated Volume (L)
        public float? STarget { get; set; } // Substitution Goal (L)
        public float? STemp { get; set; } // Substitution Temperature (°C)

        public float? Ktv { get; set; } // Kt/V (some machine can calculate this e.g. Nikkiso DBB EXA and EXA ES)
        public float? PRR { get; set; } // PRR (some machine can calculate this : specifically Nikkiso DBB EXA)
        public float? URR { get; set; } // URR (some machine can calculate this : specifically Nikkiso DBB EXA)
        public int? RecirculationRate { get; set; } // Recirculation rate (%) (some machine can calculate this : specifically Nikkiso DBB EXA)
        public float? DBV { get; set; } // dBV (%) (some machine has this e.g. Nikkiso DBB EXA and 07)

        public string Note { get; set; }

        public bool IsFromMachine { get; set; }

        [NotMapped]
        public HemodialysisRecord Hemodialysis { get; set; }

        [NotMapped]
        public IEnumerable<DialysisRecordAssessmentItem> AssessmentItems { get; set; }
    }
}
