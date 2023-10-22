using AutoMapper;
using System;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Core.Mapper
{
    public class HemoProfile : Profile
    {
        public HemoProfile()
        {
            CreateMap<HemodialysisRecord, HemodialysisRecord>()
                .ForMember(x => x.CreatedBy, c => c.Condition((s, d) => d.CreatedBy == Guid.Empty))
                .ForMember(x => x.ShiftSectionId, c => c.Ignore())
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));
        }
    }
}
