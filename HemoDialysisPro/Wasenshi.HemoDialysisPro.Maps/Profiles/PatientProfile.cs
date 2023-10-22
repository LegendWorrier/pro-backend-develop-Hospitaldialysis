using AutoMapper;
using AutoMapper.EquivalencyExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            CreateMap<int, Allergy>()
                .EqualityComparison((s, d) => s == d.MedicineId)
                .ConstructUsing(x => new Allergy { MedicineId = x })
                .ReverseMap()
                .ConstructUsing(x => x.MedicineId);

            CreateMap<CreatePatientViewModel, Patient>();
            CreateMap<Patient, PatientViewModel>()
                .ForMember(s => s.DoctorId, opts => opts.Condition((src, des, srcMember) => srcMember != Guid.Empty));

            CreateMap<DialysisInfoViewModel, DialysisInfo>()
                .ReverseMap();
            CreateMap<EmergencyContactViewModel, EmergencyContact>()
                .ReverseMap();
            CreateMap<TagViewModel, Tag>()
                .EqualityComparison((s, d) => s.Id == d.Id)
                .ReverseMap();

            CreateMap<EditPatientViewModel, Patient>(MemberList.None)
                .ForMember(s => s.DoctorId, c => c.NullSubstitute(Guid.Empty))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
                //.ForAllOtherMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<EditMedicinePrescriptionViewModel, MedicinePrescription>()
                .ForMember(x => x.Medicine, c => c.Ignore())
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));
            CreateMap<MedicinePrescription, MedicinePrescriptionViewModel>()
                .ForMember(x => x.IsHistory, c => c.MapFrom(x => x.MedicineRecords.Any(n => n.Hemodialysis.CompletedTime != null)));

            CreateMap<Underlying, MasterDataViewModel>()
                .ReverseMap();
            CreateMap<AdmissionUnderlying, MasterDataViewModel>()
                .EqualityComparison((s, d) => s.UnderlyingId == d.Id)
                .ConstructUsing((s, c) => c.Mapper.Map<MasterDataViewModel>(s.Underlying))
                .ReverseMap()
                .ConstructUsing(x => new AdmissionUnderlying { UnderlyingId = x.Id });
            CreateMap<Admission, AdmissionViewModel>()
                .ReverseMap();
            CreateMap<CreateAdmissionViewModel, Admission>();


            CreateMap<MedHistoryItem, MedHistoryItemViewModel>()
                .ReverseMap();
            CreateMap<MedInfoViewModel, MedHistoryItem>()
                .ForMember(x => x.EntryTime, c => c.MapFrom(s => (s.EntryTime ?? default).UtcDateTime));
            CreateMap<MedOverview, MedOverviewViewModel>();
            CreateMap<MedItem, MedItemViewModel>();

            CreateMap<KeyValuePair<Medicine, List<MedHistoryItem>[]>, KeyValuePair<MedicineViewModel, List<MedHistoryItemViewModel>[]>>()
                .ReverseMap();

            CreateMap<MedHistoryResult, MedHistoryResultViewModel>()
                .ReverseMap();

            CreateMap<PatientHistory, PatientHistoryViewModel>()
                .ReverseMap();
            
        }
    }
}
