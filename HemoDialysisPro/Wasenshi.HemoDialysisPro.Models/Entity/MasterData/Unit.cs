using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    /// <summary>
    /// Unit is a hemodialysis center where a group of beds existed in. Each Unit has its own patients, doctor, and nurses.<br></br>
    /// <br></br>
    /// Note: Users can be assigned to multiple units, but one patient will always belong to only one unit.
    /// </summary>
    public class Unit : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
        public string Code { get; set; } // Government Hospital will have unit id/code

        public Guid? HeadNurse { get; set; }

        [NotMapped]
        public ICollection<ScheduleSection> Sections { get; set; }
    }
}
