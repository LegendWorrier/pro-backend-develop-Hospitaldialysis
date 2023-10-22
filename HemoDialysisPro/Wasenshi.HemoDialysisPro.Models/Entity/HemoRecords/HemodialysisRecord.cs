using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class HemodialysisRecord : EntityBase<Guid>
    {
        [Required]
        public string PatientId { get; set; }

        public DateTime? CompletedTime { get; set; }

        [JsonConverter(typeof(JsonEnumConverter<AdmissionType>))]
        public AdmissionType Admission { get; set; } = AdmissionType.OutpatientClinic;
        public bool OutsideUnit { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public DateTime? CycleStartTime { get; set; }
        public DateTime? CycleEndTime { get; set; }

        public bool IsICU { get; set; }
        public DialysisType Type { get; set; }

        public bool AcNotUsed { get; set; }
        public string ReasonForRefraining { get; set; }

        // Planning (actual amount will be calculated from dialysis record)
        public float? FlushNSS { get; set; } // ml
        public int? FlushNSSInterval { get; set; } // interval in minutes (min)
        public int? FlushTimes { get; set; } // Flush NSS multiplied by this = Total amount (ml)

        public DehydrationRecord Dehydration { get; set; } = new DehydrationRecord();
        public Guid? DialysisPrescriptionId { get; set; }
        public DialysisPrescription DialysisPrescription { get; set; }
        public ICollection<VitalSignRecord> PreVitalsign { get; set; } = new List<VitalSignRecord>();
        public ICollection<VitalSignRecord> PostVitalsign { get; set; } = new List<VitalSignRecord>();
        public DialyzerRecord Dialyzer { get; set; } = new DialyzerRecord();
        public BloodCollectionRecord BloodCollection { get; set; } = new BloodCollectionRecord();
        public AVShuntRecord AvShunt { get; set; } = new AVShuntRecord();

        public Guid? ProofReader { get; set; }
        public bool DoctorConsent { get; set; }

        public int ShiftSectionId { get; set; }
        [Column(TypeName = "uuid[]")]
        public Guid[] NursesInShift { get; set; }
        public int? TreatmentNo { get; set; }
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// Flag to indicate whether this hemosheet has ever been sent to hospital/center system (at least once) or not.
        /// </summary>
        public bool SentPDF { get; set; }

        [NotMapped]
        public HemoNote Note { get; set; }
    }
}
