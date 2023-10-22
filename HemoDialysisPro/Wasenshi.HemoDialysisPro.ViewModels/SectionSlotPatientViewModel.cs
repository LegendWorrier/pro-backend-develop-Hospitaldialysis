using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class SectionSlotPatientViewModel : EntityViewModel
    {
        public int SectionId { get; set; }
        public string PatientId { get; set; }
        public SectionSlots Slot { get; set; }
    }
}
