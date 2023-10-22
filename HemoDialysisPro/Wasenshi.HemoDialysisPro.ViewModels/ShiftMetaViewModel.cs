using System;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ShiftMetaViewModel : EntityViewModel
    {
        public long Id { get; set; }
        public DateTimeOffset Month { get; set; }
        public int UnitId { get; set; } // flatten from schedulemeta
        public int SectionCount => ScheduleMeta.Count;

        public long ScheduleMetaId { get; set; }
        public ScheduleMetaViewModel ScheduleMeta { get; set; }
    }

    public class ScheduleMetaViewModel : EntityViewModel
    {
        public string UnitName { get; set; }
        public TimeSpan? Section1 { get; set; }
        public TimeSpan? Section2 { get; set; }
        public TimeSpan? Section3 { get; set; }
        public TimeSpan? Section4 { get; set; }
        public TimeSpan? Section5 { get; set; }
        public TimeSpan? Section6 { get; set; }

        [JsonIgnore]
        public int Count =>
            (Section1.HasValue ? 1 : 0) +
            (Section2.HasValue ? 1 : 0) +
            (Section3.HasValue ? 1 : 0) +
            (Section4.HasValue ? 1 : 0) +
            (Section5.HasValue ? 1 : 0) +
            (Section6.HasValue ? 1 : 0);
    }
}
