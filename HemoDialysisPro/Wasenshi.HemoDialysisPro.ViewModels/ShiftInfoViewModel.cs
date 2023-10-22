using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ShiftInfoViewModel
    {
        public int UnitId { get; set; }
        public int CurrentShift { get; set; } // index to sections
        public DateTimeOffset? LastStarted { get; set; }
        [JsonIgnore]
        public ScheduleSectionViewModel CurrentSection => CurrentShift < 0 ? null : Sections[CurrentShift];
        public List<ScheduleSectionViewModel> Sections { get; set; }
    }
}
