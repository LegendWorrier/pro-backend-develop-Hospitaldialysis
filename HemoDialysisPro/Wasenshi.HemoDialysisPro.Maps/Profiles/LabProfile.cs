using AutoMapper;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class LabProfile : Profile
    {
        public LabProfile()
        {
            CreateMap<LabExam, LabExamViewModel>()
                .ReverseMap();

            //CreateMap<LabExam, float?>()
            //    .ConvertUsing(x => x == null ? (float?) null : x.LabValue);

            CreateMap<KeyValuePair<LabExamItem, List<LabExam>[]>, KeyValuePair<LabExamItemViewModel, List<LabExamViewModel>[]>>()
                .ReverseMap();

            CreateMap<LabExamResult, LabExamResultViewModel>()
                .ReverseMap();

            CreateMap<LabInfoViewModel, LabExam>();

            CreateMap<LabOverview, LabOverviewViewModel>();

            CreateMap<LabHemosheet, LabExamItemViewModel>()
                .ConstructUsing((LabHemosheet s, ResolutionContext c) => c.Mapper.Map<LabExamItemViewModel>(s.Item));

            CreateMap<LabHemosheetViewModel, LabHemosheet>()
                .ReverseMap();
        }
    }
}
