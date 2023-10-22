using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Services.Core.Mapper
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            CreateMap<MedicineStock, StockItem>();
            CreateMap<MedicalSupplyStock, StockItem>();
            CreateMap<DialyzerStock, StockItem>();
            CreateMap<EquipmentStock, StockItem>();
        }
    }
}
