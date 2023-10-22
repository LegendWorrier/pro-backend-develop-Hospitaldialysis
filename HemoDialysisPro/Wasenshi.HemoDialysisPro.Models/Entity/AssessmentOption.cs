namespace Wasenshi.HemoDialysisPro.Models
{
    public class AssessmentOption : EntityBase<long>
    {
        public long AssessmentId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Note { get; set; }
        public bool IsDefault { get; set; }

        // ===== For Text/Number type ========
        public string TextValue { get; set; }
        public float? Value { get; set; }
    }
}