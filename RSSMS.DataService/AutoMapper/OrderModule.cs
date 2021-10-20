﻿using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Orders;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderModule
    {
        public static void ConfigOrderModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Order, OrderStorageViewModel>()
                .ForMember(des => des.RemainingTime, opt => opt.MapFrom(src => (src.ReturnDate - DateTime.Now).Value.Days));
            mc.CreateMap<OrderStorageViewModel, Order>();

            mc.CreateMap<Order, OrderCreateViewModel>();
            mc.CreateMap<OrderCreateViewModel, Order>()
                  .ForMember(des => des.Status, opt => opt.MapFrom(src => 1))
                  .ForMember(des => des.IsActive, opt => opt.MapFrom(src => 1));

            mc.CreateMap<Order, OrderViewModel>()
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate - DateTime.Now).Value.Days + 1))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => ((src.ReturnDate - DateTime.Now).Value.Days + 1) / 30));

            mc.CreateMap<OrderViewModel, Order>();

            mc.CreateMap<Order, OrderUpdateViewModel>();
            mc.CreateMap<OrderUpdateViewModel, Order>();
        }
    }
}
