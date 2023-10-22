using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Patient : EntityBase<string>
    {
        [Required]
        public string HospitalNumber { get; set; }

        public string IdentityNo { get; set; } // Passport No / Id Card No
        [Required]
        public string Name { get; set; }
        public string Gender { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }
        public string BloodType { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }

        public string TransferFrom { get; set; }

        [JsonConverter(typeof(JsonEnumConverter<AdmissionType?>))]
        public AdmissionType? Admission { get; set; }
        [JsonConverter(typeof(JsonEnumConverter<CoverageSchemeType?>))]
        public CoverageSchemeType? CoverageScheme { get; set; } //สิทธิการเบิกจ่าย

        public Guid? DoctorId { get; set; }

        [Required]
        public int UnitId { get; set; }

        public string Note { get; set; }

        public DialysisInfo DialysisInfo { get; set; }
        public EmergencyContact EmergencyContact { get; set; }

        public ICollection<Allergy> Allergy { get; set; }

        public ICollection<Tag> Tags { get; set; }

        // Others
        public string Barcode { get; set; }
        public string RFID { get; set; }
    }
}
