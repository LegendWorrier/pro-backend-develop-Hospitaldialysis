using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Users;

namespace Wasenshi.HemoDialysisPro.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<int, UserUnit>()
                .EqualityComparison((s, d) => s == d.UnitId)
                .ConvertUsing(x => new UserUnit { UnitId = x });

            CreateMap<IUser, User>()
                .ForMember(x => x.RefreshTokens, c => c.Ignore())
                .Include<User, UserPwdHashHidden>();

            CreateMap<RegisterViewModel, User>()
                .ForMember(d => d.PasswordHash, x => x.Ignore())
                .ForMember(d => d.Units, x => x.MapFrom(x => x.Units))
                .ReverseMap();

            CreateMap<LoginViewModel, User>()
                .ReverseMap();

            CreateMap<EditUserViewModel, User>()
                .ForMember(d => d.PasswordHash, x => x.Ignore())
                .ForMember(d => d.Signature, x => x.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserResult, PwdHashHiddenResult>();
            CreateMap<User, UserPwdHashHidden>()
                .ForMember(d => d.Units, x => x.MapFrom(s => s.Units));
            CreateMap<UserUnit, UserUnit>();

            CreateMap<Role, RoleViewModel>();

            
        }
    }
}
