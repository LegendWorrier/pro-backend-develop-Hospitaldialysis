using FluentValidation;
using System;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class CreatePatientViewValidator : AbstractValidator<CreatePatientViewModel>
    {
        public CreatePatientViewValidator()
        {
            RuleFor(x => x.UnitId).NotEmpty();
            RuleFor(x => x.Id).NotEmpty().MinimumLength(3);
            RuleFor(x => x.HospitalNumber).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .LessThan(DateTime.Now.AddYears(-1)); //Must be realistic birthDate
            RuleFor(x => x.Admission).IsEnumName(typeof(AdmissionType), false);
            RuleFor(x => x.CoverageScheme).IsEnumName(typeof(CoverageSchemeType), false);
        }
    }
}
