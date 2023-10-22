using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class CreatePatientViewModel
    {
        public string Id { get; set; }
        public string HospitalNumber { get; set; }
        public string IdentityNo { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BloodType { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }

        public string TransferFrom { get; set; }
        public string Admission { get; set; }
        public string CoverageScheme { get; set; }

        public DialysisInfoViewModel DialysisInfo { get; set; }
        public EmergencyContactViewModel EmergencyContact { get; set; }

        public string Note { get; set; }

        public Guid? DoctorId { get; set; }
        public int UnitId { get; set; }

        public string Barcode { get; set; }
        public string RFID { get; set; }

        public ICollection<TagViewModel> Tags { get; set; }

        public ICollection<int> Allergy { get; set; }
    }

    public class EmergencyContactViewModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Relationship { get; set; }
    }

    public class DialysisInfoViewModel
    {
        public int? AccumulatedTreatmentTimes { get; set; }
        public DateTimeOffset? FirstTime { get; set; }
        public DateTimeOffset? FirstTimeAtHere { get; set; }
        public DateTimeOffset? EndDateAtHere { get; set; }
        public DateTimeOffset? KidneyTransplant { get; set; }
        public KidneyState? KidneyState { get; set; }
        public string CauseOfKidneyDisease { get; set; }
        public string Status { get; set; }

        // Deceased
        public DateTimeOffset? TimeOfDeath { get; set; }
        public string CauseOfDeath { get; set; }

        // Transfer
        public string TransferTo { get; set; }
    }
}
