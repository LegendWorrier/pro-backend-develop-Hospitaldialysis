using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class DialysisRecordViewValidator : AbstractValidator<DialysisRecordViewModel>
    {
        public DialysisRecordViewValidator()
        {
            RuleFor(x => x.HemodialysisId).NotEmpty();
            RuleFor(x => x.Timestamp).NotEmpty();
        }
    }
}
