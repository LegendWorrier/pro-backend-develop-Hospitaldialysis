using AutoMapper;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Report.Models;

namespace Wasenshi.HemoDialysisPro.Report
{
    public class ReportMapProfile : Profile
    {
        public ReportMapProfile()
        {
            CreateMap<HemodialysisRecord, HemosheetData>()
                .ForMember(x => x.NursesInShift, c => c.Ignore());
            CreateMap<HemosheetData, HemosheetData>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .DisableCtorValidation();

            CreateMap<DehydrationRecord, DehydrationData>();
            CreateMap<DialysisPrescription, Models.DialysisPrescriptionData>();
            CreateMap<Patient, PatientData>();
            CreateMap<DialysisRecord, DialysisRecordData>();
            CreateMap<DialysisRecordData, DialysisRecordData>();

            CreateMap<AssessmentItem, AssessmentData>()
                .ForMember(x => x.Selected, c => c.Ignore());
            CreateMap<DialysisRecordAssessmentItem, AssessmentData>()
                .ForMember(x => x.IsReassessment, c => c.Ignore())
                .ForMember(x => x.Selected, c => c.Ignore());

            CreateMap<Admission, AdmissionData>();

            CreateMap<ProgressNote, ProgressNoteData>()
                .ConstructUsingServiceLocator();

            CreateMap<NurseRecord, NurseRecordData>()
                .ConstructUsingServiceLocator();

            CreateMap<DoctorRecord, DoctorRecordData>()
                .ConstructUsingServiceLocator();

            CreateMap<MedicineRecord, MedicineRecordData>()
                .ConstructUsingServiceLocator();

            CreateMap<ExecutionRecord, MedicineRecordData>()
                .Include<MedicineRecord, MedicineRecordData>()
                .ConstructUsingServiceLocator();

            CreateMap<HemoRecordData, HemoRecordData>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .DisableCtorValidation();
            CreateMap<HemosheetInfo, HemosheetInfo>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
            CreateMap<DehydrationData, DehydrationData>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
            CreateMap<HemodialysisRecord, HemosheetInfo>()
                .ForMember(x => x.NursesInShift, c => c.Ignore());
        }
    }
}
