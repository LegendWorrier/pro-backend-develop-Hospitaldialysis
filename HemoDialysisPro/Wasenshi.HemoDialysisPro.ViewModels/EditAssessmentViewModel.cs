using AutoMapper.Configuration.Annotations;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditAssessmentViewModel : AssessmentViewModel
    {
        [Ignore]
        public int Order { get; set; }
        [Ignore]
        public new AssessmentTypes Type { get; set; }
    }
}
