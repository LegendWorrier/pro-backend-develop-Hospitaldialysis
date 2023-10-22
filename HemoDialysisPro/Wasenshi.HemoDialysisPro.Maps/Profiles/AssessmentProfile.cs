using AutoMapper;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class AssessmentProfile : Profile
    {
        public AssessmentProfile()
        {
            CreateMap<CreateAssessmentViewModel, Assessment>();
            CreateMap<EditAssessmentViewModel, Assessment>()
                .ForMember(x => x.Order, c => c.Ignore())
                .ForMember(x => x.Type, c => c.Ignore())
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));
            CreateMap<Assessment, AssessmentViewModel>();

            CreateMap<CreateAssessmentGroupViewModel, AssessmentGroup>();
            CreateMap<EditAssessmentGroupViewModel, AssessmentGroup>()
                .ForMember(x => x.Type, c => c.Ignore())
                .ForAllMembers(c => c.Condition((s, d, sm) => sm != null && !sm.Equals(Guid.Empty)));
            CreateMap<AssessmentGroup, AssessmentGroupViewModel>();

            CreateMap<AssessmentOptionViewModel, AssessmentOption>()
                .ReverseMap();

            CreateMap<AssessmentItemViewModel, AssessmentItem>()
                .ReverseMap();

            CreateMap<AssessmentItemViewModel, DialysisRecordAssessmentItem>()
                .ReverseMap();

            CreateMap<Assessment, AssessmentInfo>();
        }
    }
}
