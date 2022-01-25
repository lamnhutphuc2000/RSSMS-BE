using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderHistoryExtensionModule
    {
        public static void ConfigOrderHistoryExtensionModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<RequestCreateViewModel, OrderHistoryExtension>()
                .ForMember(des => des.CreateDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
