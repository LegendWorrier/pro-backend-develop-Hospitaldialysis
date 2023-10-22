using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class RegisterViewValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewValidator()
        {
            RuleFor(x => x.Units).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
        }
    }
}
