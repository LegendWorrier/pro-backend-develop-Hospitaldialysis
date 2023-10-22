using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class CreateMedicinePrescriptionViewValidator : AbstractValidator<EditMedicinePrescriptionViewModel>
    {
        public CreateMedicinePrescriptionViewValidator()
        {
            RuleFor(x => x.MedicineId).NotEmpty();
        }
    }
}
