using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.JWT;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class AccountModule
    {
        public static void ConfigAccountModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Account, AccountsViewModel>();
            mc.CreateMap<AccountsViewModel, Account>();

            mc.CreateMap<AccountsCreateThirdPartyViewModel, Account>();
            mc.CreateMap<Account, AccountsCreateThirdPartyViewModel>();

            mc.CreateMap<AccountsCreateViewModel, Account>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => DateTime.Now));

            mc.CreateMap<AccountsUpdateViewModel, Account>();
            mc.CreateMap<Account, AccountsUpdateViewModel>();

            mc.CreateMap<Account, TokenViewModel>()
                .ForMember(des => des.UserId, opt => opt.MapFrom(src => src.Id));
            mc.CreateMap<TokenGenerateViewModel, TokenViewModel>();


        }
    }
}
