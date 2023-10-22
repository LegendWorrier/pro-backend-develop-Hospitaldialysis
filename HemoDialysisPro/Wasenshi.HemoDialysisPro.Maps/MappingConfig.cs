using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public static class MappingConfig
    {
        public static IServiceCollection AddMappingConfig(this IServiceCollection services, Action<IMapperConfigurationExpression> extraConfig = null)
        {
            services.AddAutoMapper(x =>
            {
                if (extraConfig != null)
                {
                    extraConfig(x);
                }
            }, typeof(UserProfile));

            return services;
        }

        public static IServiceCollection AddMappingConfig<TDbContext>(this IServiceCollection services, Action<IMapperConfigurationExpression> extraConfig = null) where TDbContext : DbContext
        {
            services.AddAutoMapper(x =>
            {
                x.AddCollectionMappers();
                x.UseEntityFrameworkCoreModel<TDbContext>(services);
                x.CreateMap<DateTimeOffset, DateTime>().ConvertUsing(d => d.UtcDateTime);
                x.CreateMap<DateTime, DateTimeOffset>().ConstructUsing(d => new DateTimeOffset(d, TimeSpan.Zero));
                x.CreateMap<DateTime, DateOnly>().ConstructUsing(d => DateOnly.FromDateTime(d));
                x.CreateMap<DateOnly, DateTime>().ConstructUsing(d => d.ToDateTime(new TimeOnly(0, 0)));
                x.CreateMap<int, TimeOnly>().ConstructUsing(d => new TimeOnly(0, 0).AddMinutes(d));
                x.CreateMap<TimeOnly, int>().ConstructUsing(d => d.Hour * 60 + d.Minute);
                x.CreateMap<DateTimeOffset, DateOnly>().ConstructUsing(d => DateOnly.FromDateTime(d.DateTime));
                if (extraConfig != null)
                {
                    extraConfig(x);
                }
            }, typeof(UserProfile));

            return services;
        }
    }
}
