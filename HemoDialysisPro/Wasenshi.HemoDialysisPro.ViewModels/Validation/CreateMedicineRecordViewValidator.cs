using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class CreateMedicineRecordViewValidator : AbstractValidator<CreateMedicineRecordViewModel>
    {
        public CreateMedicineRecordViewValidator()
        {
            RuleFor(x => x.Prescriptions).NotEmpty();
        }
    }
}
