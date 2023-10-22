using AutoMapper.Configuration.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class MedicinePrescription : EntityBase<Guid>
    {
        [Required]
        public string PatientId { get; set; }
        [Required]
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        [Required]
        public UsageWays Route { get; set; }
        [Required]
        public Frequency Frequency { get; set; }
        [Required]
        public DateTime AdministerDate { get; set; } // วันที่สั่งยา

        public int Duration { get; set; } // 0 = No expiration date (unit = days)

        public string HospitalName { get; set; } // in case of Medicine from Outside

        public float? OverrideDose { get; set; }
        public string OverrideUnit { get; set; }

        public string Note { get; set; }

        [NotMapped] public Medicine Medicine { get; set; }

        [Ignore]
        [IgnoreDataMember]
        [NotMapped]
        public ICollection<MedicineRecord> MedicineRecords { get; set; }
    }
}
