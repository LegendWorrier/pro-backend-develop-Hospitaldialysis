using AutoMapper;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps.Profiles
{
    public class RecordProfile : Profile
    {
        public RecordProfile()
        {
            CreateMap<DialysisRecordViewModel, DialysisRecord>()
                .ReverseMap();
            CreateMap<NurseRecordViewModel, NurseRecord>()
                .ReverseMap();
            CreateMap<DoctorRecordViewModel, DoctorRecord>()
                .ReverseMap();


            CreateMap<CreateMedicineRecordViewModel, MedicineRecord>();

            CreateMap<CreateExecutionRecordViewModel, FlushRecord>();
            CreateMap<CreateExecutionRecordViewModel, MedicineRecord>();

            
            CreateMap<ExecutionRecord, ExecutionRecordViewModel>()
                .Include<MedicineRecord, ExecutionRecordViewModel>()
                .Include<FlushRecord, ExecutionRecordViewModel>();

            CreateMap<FlushRecord, ExecutionRecordViewModel>();
            CreateMap<MedicineRecord, ExecutionRecordViewModel>();

            CreateMap<EditExecutionRecordViewModel, ExecutionRecord>()
                .Include<EditExecutionRecordViewModel, MedicineRecord>()
                .Include<EditExecutionRecordViewModel, FlushRecord>()
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));

            CreateMap<EditExecutionRecordViewModel, MedicineRecord>();
            CreateMap<EditExecutionRecordViewModel, FlushRecord>();

            CreateMap<DialysisData, DialysisRecord>();
            CreateMap<DialysisRecord, DialysisRecord>();

            CreateMap<ProgressNote, ProgressNoteViewModel>()
                .ReverseMap();
        }
    }
}
