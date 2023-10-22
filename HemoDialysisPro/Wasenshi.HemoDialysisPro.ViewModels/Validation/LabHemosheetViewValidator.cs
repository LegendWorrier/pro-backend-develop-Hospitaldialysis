using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class LabHemosheetViewValidator : AbstractValidator<LabHemosheetViewModel>
    {
        public LabHemosheetViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.LabItemId).NotEmpty();
        }
    }
}
