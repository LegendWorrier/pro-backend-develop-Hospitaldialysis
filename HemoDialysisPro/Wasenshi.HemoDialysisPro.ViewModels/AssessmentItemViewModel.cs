using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AssessmentItemViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public long AssessmentId { get; set; }
        public bool IsReassessment { get; set; }
        public long[] Selected { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public float? Value { get; set; }
    }
}
