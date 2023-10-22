using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Share
{
    public class UnitShift
    {
        public static string GetKey(int unitId) => $"unitshift-{unitId}";

        public int Id { get; set; } // Unit Id
        public int CurrentShift { get; set; } // index number
        public DateTime? LastStarted { get; set; }
        [JsonIgnore]
        public ScheduleSection CurrentSection => CurrentShift < 0 ? null : Sections[CurrentShift];
        [JsonIgnore]
        public ScheduleSection NextSection => CurrentShift == Sections.Count - 1 ? null : Sections[CurrentShift + 1];

        public List<ScheduleSection> Sections { get; set; }
    }
}
