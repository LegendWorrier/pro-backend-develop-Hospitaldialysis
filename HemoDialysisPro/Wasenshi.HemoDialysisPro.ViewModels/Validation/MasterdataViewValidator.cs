using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class MasterdataViewValidator : AbstractValidator<MasterDataViewModel>
    {
        public MasterdataViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
