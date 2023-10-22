using AutoMapper;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class MonitoringProfile : Profile
    {
        public MonitoringProfile()
        {
            CreateMap<Bed, BedViewModel>()
                .ReverseMap();
            CreateMap<BedViewModel, BedViewModel>();
            CreateMap<BedBoxInfo, BedViewModel>();
        }
    }
}
