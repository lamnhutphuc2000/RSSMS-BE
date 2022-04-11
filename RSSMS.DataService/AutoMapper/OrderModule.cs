using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Orders;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderModule
    {
        public static void ConfigOrderModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderCreateViewModel, Order>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => TimeUtilStatic.StringToTime(src.DeliveryTime)))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => 1))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Order, OrderViewModel>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime)))
                .ForMember(des => des.Status, opt => opt.MapFrom(src => (src.ReturnDate - DateTime.Now).Value.Days > 0 ? (int?)(src.ReturnDate - DateTime.Now).Value.Days <= 3 ? 3 : src.Status : 4))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));

            mc.CreateMap<Order, OrderByIdViewModel>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime)))
                .ForMember(des => des.Status, opt => opt.MapFrom(src => (src.ReturnDate - DateTime.Now).Value.Days > 0 ? (int?)(src.ReturnDate - DateTime.Now).Value.Days <= 3 ? 3 : src.Status : 4))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));


            mc.CreateMap<Order, OrderUpdateViewModel>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime)))
                .ForMember(des => des.ReturnTime, opt => opt.MapFrom(src => TimeUtilStatic.TimeToString((TimeSpan)src.ReturnTime)))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            mc.CreateMap<OrderUpdateViewModel, Order>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => TimeUtilStatic.StringToTime(src.DeliveryTime)))
                .ForMember(des => des.ReturnTime, opt => opt.MapFrom(src => TimeUtilStatic.StringToTime(src.ReturnTime)))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            mc.CreateMap<OrderCreateViewModel, OrderByIdViewModel>();

        }
    }
}
