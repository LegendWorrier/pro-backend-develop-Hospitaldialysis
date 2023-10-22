using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class AdmissionViewValidator : AbstractValidator<AdmissionViewModel>
    {
        public AdmissionViewValidator()
        {
            RuleFor(x => x.AN).NotEmpty();
            RuleFor(x => x.Admit).NotEmpty();
        }
    }
}
