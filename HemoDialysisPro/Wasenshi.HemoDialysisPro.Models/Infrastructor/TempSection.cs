using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    /// <summary>
    /// Each section has 4 hours. In one day, there will be 3 - 5 sections normally. (max 6)
    /// </summary>
    public class TempSection : IEntityBase<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int UnitId { get; set; }
        [Column(TypeName = "time")]
        public TimeSpan? StartTime { get; set; }

        public bool Delete { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
