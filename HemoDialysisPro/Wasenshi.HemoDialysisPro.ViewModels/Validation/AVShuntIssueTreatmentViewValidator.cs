using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class AVShuntIssueTreatmentViewValidator : AbstractValidator<AVShuntIssueTreatmentViewModel>
    {
        public AVShuntIssueTreatmentViewValidator()
        {
            RuleFor(x => x.AbnormalDatetime).NotEmpty();
            RuleFor(x => x.Complications).NotEmpty();
            RuleFor(x => x.TreatmentMethod).NotEmpty();
            RuleFor(x => x.TreatmentResult).NotEmpty();
        }
    }
}
