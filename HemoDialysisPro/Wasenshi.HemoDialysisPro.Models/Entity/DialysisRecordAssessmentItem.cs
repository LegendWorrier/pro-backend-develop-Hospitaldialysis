using System;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class DialysisRecordAssessmentItem : AssessmentItemCommon
    {
        public Guid DialysisRecordId { get; set; }

        [NotMapped]
        public DialysisRecord DialysisRecord { get; set; }

    }
}
