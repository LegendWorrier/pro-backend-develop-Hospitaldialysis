using AutoMapper;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            CreateMap<StockItemViewModel, MedicineStock>()
                .ReverseMap();
            CreateMap<StockItemViewModel, MedicalSupplyStock>()
                .ReverseMap();
            CreateMap<StockItemViewModel, EquipmentStock>()
                .ReverseMap();
            CreateMap<StockItemViewModel, DialyzerStock>()
                .ReverseMap();

            CreateMap<Guid, MedicineStock>()
                .ConvertUsing((Guid id, MedicineStock _) => new MedicineStock { Id = id });
            CreateMap<Guid, MedicalSupplyStock>()
                .ConvertUsing((Guid id, MedicalSupplyStock _) => new MedicalSupplyStock { Id = id });
            CreateMap<Guid, EquipmentStock>()
                .ConvertUsing((Guid id, EquipmentStock _) => new EquipmentStock { Id = id });
            CreateMap<Guid, DialyzerStock>()
                .ConvertUsing((Guid id, DialyzerStock _) => new DialyzerStock { Id = id });

            CreateMap<StockItemWithTypeViewModel, MedicineStock>();
            CreateMap<StockItemWithTypeViewModel, MedicalSupplyStock>();
            CreateMap<StockItemWithTypeViewModel, EquipmentStock>();
            CreateMap<StockItemWithTypeViewModel, DialyzerStock>();

        }
    }
}
