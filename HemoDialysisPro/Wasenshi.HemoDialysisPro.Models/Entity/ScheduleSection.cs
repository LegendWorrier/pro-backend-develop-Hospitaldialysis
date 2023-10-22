using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    /// <summary>
    /// Each section has 4 hours. In one day, there will be 3 - 5 sections normally. (max 6)
    /// </summary>
    public class ScheduleSection : EntityBase<int>
    {
        public int UnitId { get; set; }

        public TimeOnly StartTime { get; set; }

        [NotMapped]
        public Unit Unit { get; set; }
    }
}
