using Microsoft.EntityFrameworkCore;
using System;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class DialysisInfo
    {
        public int? AccumulatedTreatmentTimes { get; set; }
        public DateTime? FirstTime { get; set; }
        public DateTime? FirstTimeAtHere { get; set; }
        public DateTime? EndDateAtHere { get; set; }
        public DateTime? KidneyTransplant { get; set; }
        public KidneyState? KidneyState { get; set; }
        public string CauseOfKidneyDisease { get; set; }
        public string Status { get; set; }

        // Deceased
        public DateTime? TimeOfDeath { get; set; }
        public string CauseOfDeath { get; set; }

        // Transfer
        public string TransferTo { get; set; }
    }
}