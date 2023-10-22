using AutoMapper;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class HemodialysisProfile : Profile
    {
        public HemodialysisProfile()
        {
            CreateMap<int, TimeSpan>()
                .ConstructUsing(d => TimeSpan.FromMinutes(d))
                .ReverseMap()
                .ConstructUsing(t => (int)t.TotalMinutes);

            CreateMap<EditHemodialysisRecordViewModel, HemodialysisRecord>()
                .ForMember(x => x.DialysisPrescription, c => c.Ignore())
                .ForMember(x => x.IsActive, c => c.MapFrom((x) => true))
                .ForMember(x => x.Admission, c => c.PreCondition((s) => s.Admission != null))
                .ForMember(x => x.OutsideUnit, c => c.PreCondition(s => s.OutsideUnit != null))
                .ForMember(x => x.ShiftSectionId, c => c.PreCondition((s) => s.ShiftSectionId != null))
                .ForMember(x => x.Created, c => c.Ignore())
                // history parts
                .ForMember(x => x.DoctorConsent, c => c.Ignore())
                .ForMember(x => x.DoctorId, c => c.Ignore())
                .ForMember(x => x.TreatmentNo, c => c.Ignore())
                .ForMember(x => x.ShiftSectionId, c => c.Ignore())
                .ForMember(x => x.NursesInShift, c => c.Ignore())
                // all others
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));

            CreateMap<HemodialysisRecordViewModel, HemodialysisRecord>()
                .ReverseMap()
                .ForMember(x => x.DialysisPrescription, c =>
                {
                    c.Ignore();
                    c.Condition(x => x.DialysisPrescriptionId != null && x.DialysisPrescription != null);
                    c.MapFrom(x => x.DialysisPrescription);
                });

            CreateMap<DehydrationRecordViewModel, DehydrationRecord>()
                .ReverseMap();
            CreateMap<VitalSignRecordViewModel, VitalSignRecord>()
                .ReverseMap();
            CreateMap<BloodCollectionRecordViewModel, BloodCollectionRecord>()
                .ReverseMap();
            CreateMap<DialyzerRecordViewModel, DialyzerRecord>()
                .ReverseMap();
            CreateMap<AVShuntRecordViewModel, AVShuntRecord>()
                .ReverseMap();

            CreateMap<DialysisPrescription, DialysisPrescriptionViewModel>()
                .ForMember(x => x.IsHistory, c => c.MapFrom(x => x.HemodialysisRecords.Any(h => h.CompletedTime != null)))
                .ReverseMap();

            CreateMap<EditDialysisPrescriptionViewModel, DialysisPrescription>()
                .ForMember(x => x.IsActive, c => c.MapFrom((x) => true));

            CreateMap<EditAVShuntViewModel, AVShunt>();
            CreateMap<AVShunt, AVShuntViewModel>();

            CreateMap<AVShuntIssueTreatmentViewModel, AVShuntIssueTreatment>()
                .ReverseMap();

            CreateMap<HemoRecordResult, HemoResultViewModel>()
                .ForMember(x => x.Record, c => c.MapFrom(x => x.Record))
                .ForPath(x => x.Record.DialysisPrescription, c => c.MapFrom(x => x.Prescription));

            CreateMap<HemoNote, HemoNoteViewModel>()
                .ReverseMap();
        }
    }
}
