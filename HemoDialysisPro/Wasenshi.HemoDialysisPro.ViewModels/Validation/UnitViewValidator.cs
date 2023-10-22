using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class UnitViewValidator : AbstractValidator<UnitViewModel>
    {
        public UnitViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
