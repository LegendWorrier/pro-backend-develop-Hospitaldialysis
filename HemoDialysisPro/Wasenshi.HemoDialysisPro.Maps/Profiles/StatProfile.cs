using AutoMapper;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class StatProfile : Profile
    {
        public StatProfile()
        {
            CreateMap<Column, ColumnViewModel>();
            CreateMap(typeof(DataRow<>), typeof(DataRowViewModel<>));
            CreateMap(typeof(TableResult<>), typeof(TableResultViewModel<>));
            CreateMap<StatInfo, StatInfo>();
            CreateMap<StatRowInfo, StatRowInfo>();
        }
    }
}
