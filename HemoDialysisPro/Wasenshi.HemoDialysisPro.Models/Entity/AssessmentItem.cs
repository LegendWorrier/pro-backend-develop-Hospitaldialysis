using System;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class AssessmentItem : AssessmentItemCommon
    {
        public Guid HemosheetId { get; set; }
        public bool IsReassessment { get; set; }

        [NotMapped]
        public HemodialysisRecord Hemosheet { get; set; }
        
    }
}
