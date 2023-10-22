using Newtonsoft.Json;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditMedicinePrescriptionViewModel : MedicinePrescriptionViewModel
    {
        [JsonIgnore]
        public new MedicineViewModel Medicine { get; set; }

        public int MedicineId { get; set; }
    }
}