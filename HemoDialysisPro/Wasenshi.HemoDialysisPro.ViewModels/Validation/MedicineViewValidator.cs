using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class MedicineViewValidator : AbstractValidator<MedicineViewModel>
    {
        public MedicineViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
