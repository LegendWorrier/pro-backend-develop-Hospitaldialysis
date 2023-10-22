using FluentValidation;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class PrescriptionViewValidator : AbstractValidator<DialysisPrescriptionViewModel>
    {
        public PrescriptionViewValidator()
        {
            RuleFor(x => x.PatientId).NotEmpty();
            RuleFor(x => x.Duration).NotEmpty();
            RuleFor(x => x.Mode).IsEnumName(typeof(DialysisMode), false);
            RuleFor(x => x.HdfType).IsEnumName(typeof(HdfType), false);
        }
    }
}
