using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class RoleViewValidator : AbstractValidator<RoleViewModel>
    {
        public RoleViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
