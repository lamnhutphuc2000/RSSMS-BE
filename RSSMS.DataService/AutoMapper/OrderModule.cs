﻿using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Orders;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderModule
    {
        public static void ConfigOrderModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Order, OrderStorageViewModel>()
                .ForMember(des => des.RemainingTime, opt => opt.MapFrom(src => (src.ReturnDate - DateTime.Now).Value.Days));
            mc.CreateMap<OrderStorageViewModel, Order>();
                

        }
    }
}