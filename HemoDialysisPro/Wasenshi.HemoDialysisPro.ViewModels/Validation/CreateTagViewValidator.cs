using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class CreateTagViewValidator : AbstractValidator<TagViewModel>
    {
        public CreateTagViewValidator()
        {
            RuleFor(x => x.Text).NotEmpty();
            RuleFor(x => x.Color).NotEmpty();
        }
    }
}
