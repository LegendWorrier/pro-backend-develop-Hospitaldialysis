using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    /// <summary>
    /// A ShiftMeta is belonged to a single unit schedule
    /// </summary>
    public class ShiftMeta : EntityBase<long>
    {
        public DateOnly Month { get; set; }

        public long ScheduleMetaId { get; set; }

        [NotMapped]
        public ScheduleMeta ScheduleMeta { get; set; } // All unit and meta data will be stored here
    }
}
