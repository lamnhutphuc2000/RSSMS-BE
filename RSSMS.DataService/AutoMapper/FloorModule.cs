using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.Floors;
using RSSMS.DataService.ViewModels.JWT;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class FloorModule
    {
        public static void ConfigFloorModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Floor, FloorInSpaceViewModel>()
                .ForMember(des => des.Usage, opt => opt.MapFrom(src => (double)0));

            mc.CreateMap<Floor, FloorGetByIdViewModel>();
        }
    }
}
