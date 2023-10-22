namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class LabHemosheetViewModel
    {
        public string Name { get; set; }
        public int LabItemId { get; set; }
        public bool OnlyOnDate { get; set; }

        // ==== Info ========

        public string ItemName { get; set; }
        public bool ItemIsYesNo { get; set; }
    }
}
