namespace Wasenshi.HemoDialysisPro.Models
{
    public class HemoRecordResult
    {
        public HemodialysisRecord Record { get; set; }
        public Patient Patient { get; set; }
        public DialysisPrescription Prescription { get; set; }
        public HemoNote Note { get; set; }
    }
}
