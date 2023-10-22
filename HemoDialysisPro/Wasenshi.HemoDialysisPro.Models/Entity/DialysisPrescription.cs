using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class DialysisPrescription : DialysisPrescriptionData
    {
        [Required]
        public string PatientId { get; set; }

        [NotMapped]
        public ICollection<HemodialysisRecord> HemodialysisRecords { get; set; }
    }
}
