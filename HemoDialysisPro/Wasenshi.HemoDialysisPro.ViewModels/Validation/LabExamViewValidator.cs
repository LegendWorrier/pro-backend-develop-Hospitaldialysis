using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class LabExamViewValidator : AbstractValidator<LabExamViewModel>
    {
        public LabExamViewValidator()
        {
            RuleFor(x => x.PatientId).NotEmpty();
            RuleFor(x => x.LabItemId).NotEmpty();
            RuleFor(x => x.EntryTime).NotEmpty();
        }
    }
}
