using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailServiceMapModule
    {
        public static void ConfigOrderDetailServiceMapModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetailServiceViewModel, OrderDetailServiceMap>();

            mc.CreateMap<OrderDetailServiceMap, OrderDetailServiceByIdViewModel>()
                .ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.Service.Type))
                .ForMember(des => des.ServiceUrl, opt => opt.MapFrom(src => src.Service.ImageUrl));
        }
    }
}
