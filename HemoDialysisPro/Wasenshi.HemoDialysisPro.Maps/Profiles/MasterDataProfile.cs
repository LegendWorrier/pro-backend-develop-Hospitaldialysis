using AutoMapper;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class MasterDataProfile : Profile
    {
        public MasterDataProfile()
        {
            CreateMap<UnitViewModel, Unit>()
                .ReverseMap();

            CreateMap<MedicineViewModel, Medicine>()
                .ForMember(x => x.Category, c => c.Ignore())
                .ReverseMap()
                .ForMember(x => x.Category, c => c.MapFrom(x => x.Category.Name));

            CreateMap<DialyzerViewModel, Dialyzer>()
                .ReverseMap();

            CreateMap<StockableViewModel, MedicalSupply>()
                .ReverseMap();

            CreateMap<StockableViewModel, Equipment>()
                .ReverseMap();

            CreateMap<MasterDataViewModel, MedCategory>()
                .ReverseMap();

            CreateMap<StatusViewModel, Status>()
                .ReverseMap();

            CreateMap<MasterDataViewModel, DeathCause>()
                .ReverseMap();

            CreateMap<MasterDataViewModel, Anticoagulant>()
                .ReverseMap();

            CreateMap<DialysateViewModel, Dialysate>()
                .ReverseMap();

            CreateMap<NeedleViewModel, Needle>()
                .ReverseMap();

            CreateMap<LabExamItem, LabExamItemViewModel>()
                .ReverseMap()
                .ForMember(x => x.IsSystemBound, c => c.Ignore())
                .ForMember(x => x.Bound, c => c.Ignore());

            CreateMap<PatientHistoryItem, PatientHistoryItemViewModel>()
                .ReverseMap();
            CreateMap<PatientChoice, PatientChoiceViewModel>()
                .ReverseMap();

            CreateMap<WardViewModel, Ward>()
                .ReverseMap();

            CreateMap<Stockable, StockableWithTypeViewModel>()
                .ForMember(x => x.Type, c => c.MapFrom((s, d) =>
                {
                    if (s is Medicine)
                    {
                        return "med";
                    }
                    if (s is MedicalSupply)
                    {
                        return "supply";
                    }
                    if (s is Equipment)
                    {
                        return "equipment";
                    }
                    if (s is Dialyzer)
                    {
                        return "dialyzer";
                    }

                    throw new InvalidOperationException("Unhandled stockable type in mapping");
                }));
        }
    }
}
