using FluentValidation;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class StatusViewValidator : AbstractValidator<StatusViewModel>
    {
        public StatusViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Category).NotEmpty().IsEnumName(typeof(StatusCategories), false);
        }
    }
}
