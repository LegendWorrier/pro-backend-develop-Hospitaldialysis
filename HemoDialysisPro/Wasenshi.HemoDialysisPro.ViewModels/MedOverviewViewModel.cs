using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class MedOverviewViewModel
    {
        public string PatientId { get; set; }
        public IEnumerable<MedItemViewModel> ThisMonthMeds { get; set; }
    }

    public class MedItemViewModel
    {
        public int MedId { get; set; }
        public MedicineViewModel Medicine { get; set; }
        public int Count { get; set; }
    }
}
