using AutoMapper;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class UnitManagementProfile : Profile
    {
        public UnitManagementProfile()
        {
            CreateMap<ScheduleSection, ScheduleSectionViewModel>()
                .ReverseMap();
            CreateMap<SectionSlotPatient, SectionSlotPatientViewModel>()
                .ReverseMap();
            CreateMap<ScheduleSectionViewModel, TempSection>()
                .ForMember(x => x.Delete, c => c.MapFrom(_ => false))
                .ReverseMap();
            CreateMap<TempSection, ScheduleSection>();
            CreateMap<int, ScheduleSection>().ConstructUsing(x => new ScheduleSection { Id = x });
            CreateMap<int, TempSection>().ConstructUsing(x => new TempSection { Id = x, Delete = true });

            CreateMap<ScheduleSlot, ScheduleSlotViewModel>();
            CreateMap<SectionSlotPatient, PatientSlotViewModel>();

            CreateMap<SectionResult, SectionResultViewModel>();
            CreateMap<ScheduleResult, ScheduleResultViewModel>();

            CreateMap<Schedule, ScheduleViewModel>();

            CreateMap<Schedule, SchedulePatientViewModel>()
                .ForMember(x => x.OriginalSection, c => c.MapFrom(x => x.Section))
                .ForMember(x => x.OriginalSectionId, c => c.MapFrom(x => x.SectionId))
                .ForMember(x => x.OriginalSlot, c => c.MapFrom(x => x.Slot))
                .ForMember(x => x.Patient, c => c.MapFrom(x => x.Patient))
                .ForPath(x => x.Patient.Schedule, c => c.MapFrom(x => x.Date));


            CreateMap<ShiftSlot, ShiftSlotViewModel>()
                .ForMember(x => x.UnitId, c =>
                {
                    c.Condition((ctx) => (ctx.ShiftMeta?.ScheduleMeta?.UnitId ?? 0) != 0);
                    c.MapFrom(x => x.ShiftMeta.ScheduleMeta.UnitId);
                }).
                 ForMember(x => x.ShiftData, c => c.MapFrom(x => x.Data))
                .ReverseMap()
                .ForMember(x => x.ShiftMeta, c =>
                {
                    c.Condition((ctx) => ctx.UnitId != null);
                })
                .ForPath(x => x.ShiftMeta.ScheduleMeta.UnitId, c =>
                {
                    c.Ignore();
                    c.Condition((x) => x.Source.UnitId != null);
                    c.MapFrom(x => x.UnitId);
                });
            CreateMap<UserShift, UserShiftViewModel>();
            CreateMap<UserShiftEditViewModel, UserShift>();

            CreateMap<ShiftIncharge, InchargeViewModel>()
                .ReverseMap();
            CreateMap<ShiftInchargeSection, InchargeSectionViewModel>()
                .ReverseMap();

            CreateMap<ShiftMeta, ShiftMetaViewModel>()
                .ForMember(x => x.UnitId, c => c.MapFrom(x => x.ScheduleMeta.UnitId));
            CreateMap<ScheduleMeta, ScheduleMetaViewModel>();

            CreateMap<ShiftResult, ShiftResultViewModel>();
            CreateMap<UserShiftResult, UserShiftViewModel>()
                .IncludeMembers(x => x.UserShift)
                .ForMember(x => x.ShiftSlots, c => c.MapFrom(x => x.Slots));
        }
    }
}
