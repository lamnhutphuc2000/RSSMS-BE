using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.JWT;
using RSSMS.DataService.ViewModels.StaffManageUser;
using RSSMS.DataService.ViewModels.Users;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class UserModule
    {
        public static void ConfigUserModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<User, UserViewModel>();
            mc.CreateMap<UserViewModel, User>();

            mc.CreateMap<UserCreateViewModel, User>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => DateTime.Now));

            mc.CreateMap<UserUpdateViewModel, User>();
            mc.CreateMap<User, UserUpdateViewModel>();

            mc.CreateMap<User, TokenViewModel>()
                .ForMember(des => des.UserId, opt => opt.MapFrom(src => src.Id));
            mc.CreateMap<TokenGenerateModel, TokenViewModel>();

            mc.CreateMap<User, UserListStaffViewModel>();
            mc.CreateMap<UserListStaffViewModel, User>();

            mc.CreateMap<User, StaffManageStorageCreateViewModel>()
                .ForMember(des => des.UserId, opt => opt.MapFrom(src => src.Id));

            mc.CreateMap<UserCreateViewModel, UserLoginViewModel>();
        }
    }
}
