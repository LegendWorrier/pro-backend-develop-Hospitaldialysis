using AutoMapper.Configuration.Annotations;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditAssessmentGroupViewModel : AssessmentGroupViewModel
    {
        [Ignore]
        public int Order { get; set; }
        [Ignore]
        public new AssessmentTypes Type { get; set; }
    }
}
