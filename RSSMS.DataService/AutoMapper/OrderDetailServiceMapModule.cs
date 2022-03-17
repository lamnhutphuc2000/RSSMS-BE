using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailServiceMapModule
    {
        public static void ConfigOrderDetailServiceMapModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetailService, OrderDetailServiceMap>();


        }
    }
}
