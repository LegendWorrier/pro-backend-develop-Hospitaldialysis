using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class NurseRecordViewValidator : AbstractValidator<NurseRecordViewModel>
    {
        public NurseRecordViewValidator()
        {
            RuleFor(x => x.HemodialysisId).NotEmpty();
            RuleFor(x => x.Timestamp).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
    }
}
