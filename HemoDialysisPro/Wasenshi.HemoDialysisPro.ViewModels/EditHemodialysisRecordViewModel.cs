using System;
using System.Text.Json.Serialization;
using Wasenshi.AuthPolicy.Attributes;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditHemodialysisRecordViewModel : HemodialysisRecordViewModel
    {
        [JsonIgnore]
        public new DateTimeOffset? CompletedTime { get; set; }
        [JsonIgnore]
        public new DialysisPrescriptionViewModel DialysisPrescription { get; set; }
        [JsonIgnore]
        public new Guid? ProofReader { get; set; }
        [JsonIgnore]
        public new bool DoctorConsent { get; set; }
        [JsonIgnore]
        public new Guid? DoctorId { get; set; }

        [RoleRestrict(Roles.HeadNurseUp)]
        public Guid? DialysisPrescriptionId { get; set; }

        public int? ShiftSectionId { get; set; }
    }
}
