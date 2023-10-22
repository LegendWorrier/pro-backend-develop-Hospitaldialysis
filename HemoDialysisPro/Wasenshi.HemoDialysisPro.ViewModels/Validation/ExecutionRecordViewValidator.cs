using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class ExecutionRecordViewValidator : AbstractValidator<ExecutionRecordViewModel>
    {
        public ExecutionRecordViewValidator()
        {
            RuleFor(x => x.HemodialysisId).NotEmpty();
            RuleFor(x => x.Timestamp).NotEmpty();
        }
    }
}
