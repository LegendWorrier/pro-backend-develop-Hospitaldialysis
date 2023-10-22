using FluentValidation;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class HemoRecordViewValidator : AbstractValidator<HemodialysisRecordViewModel>
    {
        public HemoRecordViewValidator()
        {
            RuleFor(x => x.PatientId).NotEmpty();
            RuleFor(x => x.Admission).IsEnumName(typeof(AdmissionType), false);
        }
    }
}
