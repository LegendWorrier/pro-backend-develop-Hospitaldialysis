using System;
using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class AVShuntIssueTreatment : EntityBase<Guid>
    {
        [Required]
        public string PatientId { get; set; }
        [Required]
        public DateTime AbnormalDatetime { get; set; }
        [Required]
        public string Complications { get; set; }
        [Required]
        public string TreatmentMethod { get; set; }

        public string Hospital { get; set; }
        [Required]
        public string TreatmentResult { get; set; }

        public Guid? CathId { get; set; } //which cath does this issue involve

    }
}