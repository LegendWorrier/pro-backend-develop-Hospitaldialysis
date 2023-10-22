using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ShiftIncharge : EntityBase
    {
        [Key]
        public int UnitId { get; set; }
        [Key]
        public DateOnly Date { get; set; }
        public Guid? UserId { get; set; }

        public IEnumerable<ShiftInchargeSection> Sections { get; set; }
    }

    [Owned]
    public class ShiftInchargeSection
    {
        public int SectionId { get; set; }
        public Guid UserId { get; set; }

        [NotMapped]
        public ScheduleSection Section { get; set; }
    }
}
