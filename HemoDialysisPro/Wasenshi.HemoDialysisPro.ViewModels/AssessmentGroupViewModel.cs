using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AssessmentGroupViewModel : EntityViewModel
    {
        public int Id { get; set; }
        public AssessmentTypes Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
