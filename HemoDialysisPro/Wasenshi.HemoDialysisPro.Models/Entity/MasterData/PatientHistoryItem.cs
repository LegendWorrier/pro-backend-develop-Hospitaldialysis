using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class PatientHistoryItem : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string DisplayName { get; set; }

        [Required]
        public int Order { get; set; }

        public bool IsYesNo { get; set; }
        public bool IsNumber { get; set; }

        public IEnumerable<PatientChoice> Choices { get; set; }
        public bool AllowOther { get; set; } // whether this item can have value other than listed by choices

        public TRTMappingPatient TRT { get; set; }
    }

    [Owned]
    public class PatientChoice
    {
        public int Id { get; set; }
        public int PatientHistoryItemId { get; set; }
        public string Text { get; set; }
        public float? NumberValue { get; set; }
    }
}