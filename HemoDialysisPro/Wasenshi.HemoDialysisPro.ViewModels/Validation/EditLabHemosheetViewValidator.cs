using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class EditLabHemosheetViewValidator : AbstractValidator<LabHemosheetUpdateViewModel>
    {
        public EditLabHemosheetViewValidator()
        {
            RuleForEach(x => x.List)
                .SetValidator(new LabHemosheetViewValidator())
                .NotEmpty();
        }
    }
}
