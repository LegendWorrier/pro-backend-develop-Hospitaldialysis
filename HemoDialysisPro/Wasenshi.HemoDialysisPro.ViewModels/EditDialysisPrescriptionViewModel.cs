using System;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditDialysisPrescriptionViewModel : DialysisPrescriptionViewModel
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        [JsonIgnore]
        public bool IsActive { get; set; } = true;
    }
}