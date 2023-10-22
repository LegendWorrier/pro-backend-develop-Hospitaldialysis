using FluentValidation;
using Microsoft.Extensions.Localization;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class LabItemViewValidator : AbstractValidator<LabExamItemViewModel>
    {
        public LabItemViewValidator(IStringLocalizer<ShareResource> localizer)
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Unit).NotEmpty();
            RuleFor(x => x.Category).IsInEnum().WithMessage(x => localizer["LabItemCategoryEmpty"]);
        }
    }
}
