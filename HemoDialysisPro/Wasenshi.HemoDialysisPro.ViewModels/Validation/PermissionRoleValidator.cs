using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class PermissionRoleValidator : AbstractValidator<PermissionRole>
    {
        public PermissionRoleValidator()
        {
            RuleFor(x => x.RoleName).NotEmpty();
            RuleFor(x => x.Permissions).NotNull();
        }
    }
}
