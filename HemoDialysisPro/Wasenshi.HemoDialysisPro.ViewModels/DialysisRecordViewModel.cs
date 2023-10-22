using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class DialysisRecordViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public Guid HemodialysisId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int? Remaining { get; set; } // Total minutes
        // machine
        public string Model { get; set; }
        public string Number { get; set; }

        public int? BPS { get; set; }
        public int? BPD { get; set; }
        public int? HR { get; set; }
        public int? RR { get; set; }
        public float? Temp { get; set; } // Temperature
        public float? BFR { get; set; } // Blood Flow Rate
        public int? VP { get; set; } // Venous Pressure
        public int? AP { get; set; } // Arterial Pressure
        public int? DP { get; set; } // Dialysate Pressure
        public int? TMP { get; set; }
        public float? UFRate { get; set; } // L
        public float? UFTotal { get; set; } // L
        public float? AcLoading { get; set; }
        public float? AcMaintain { get; set; }
        public float? HAV { get; set; } // Heparin Accumulated Volume
        public float? DFRTarget { get; set; } // Dialysate Flow Rate target
        public float? DFR { get; set; } // Dialysate Flow Rate
        public string Dialysate { get; set; } // K/Ca
        public float? DTTarget { get; set; } // Dialysate Temperature target
        public float? DT { get; set; } // Dialysate Temperature
        public float? DCTarget { get; set; } // Dialysate Conductivity target
        public float? DC { get; set; } // Dialysate Conductivity
        public float? BC { get; set; } // Bicarb Conductivity
        public float? NSS { get; set; } // Saline Solution Drip (ml)
        public float? Glucose50 { get; set; } // 50% w/v (ml)
        public float? HCO3 { get; set; }
        public float? NaTarget { get; set; } // (mmole)
        public string NaProfile { get; set; }
        public string UFProfile { get; set; }
        public string Mode { get; set; }
        public float? BFAV { get; set; } // Blood Flow Accumulated Volume
        public float? UFTarget { get; set; } // UF Goal
        public float? SRate { get; set; } // Substitution Rate
        public float? SAV { get; set; } // Substitution Accumulated Volume
        public float? STarget { get; set; } // Substitution Goal 
        public float? STemp { get; set; } // Substitution Temperature

        public float? Ktv { get; set; } // Kt/V
        public float? PRR { get; set; } // PRR (some machine can calculate this : specifically Nikkiso DBB EXA)
        public float? URR { get; set; } // URR (some machine can calculate this : specifically Nikkiso DBB EXA)
        public int? RecirculationRate { get; set; } // Recirculation rate (%) (some machine can calculate this : specifically Nikkiso DBB EXA)
        public float? DBV { get; set; } // dBV (%) (some machine has this e.g. Nikkiso DBB EXA and 07)


        public string Note { get; set; }

        public bool IsFromMachine { get; set; }

        public IEnumerable<AssessmentItemViewModel> AssessmentItems { get; set; }
    }
}
