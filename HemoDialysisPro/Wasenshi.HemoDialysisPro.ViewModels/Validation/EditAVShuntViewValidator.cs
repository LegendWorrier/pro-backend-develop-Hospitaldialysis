using FluentValidation;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class EditAVShuntViewValidator : AbstractValidator<EditAVShuntViewModel>
    {
        public EditAVShuntViewValidator()
        {
            RuleFor(x => x.CatheterType).NotEmpty().IsEnumName(typeof(CatheterType), false);
            RuleFor(x => x.Side).NotEmpty().IsEnumName(typeof(SideEnum), false);
            RuleFor(x => x.ShuntSite).NotEmpty();
        }
    }
}
