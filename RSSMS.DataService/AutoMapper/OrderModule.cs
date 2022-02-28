using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Orders;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderModule
    {
        public static void ConfigOrderModule(this IMapperConfigurationExpression mc)
        {


            mc.CreateMap<Order, OrderCreateViewModel>();
            mc.CreateMap<OrderCreateViewModel, Order>()
                .ForMember(des => des.PaymentMethod, opt => opt.MapFrom(src => 0))
                  .ForMember(des => des.Status, opt => opt.MapFrom(src => 1))
                  .ForMember(des => des.IsActive, opt => opt.MapFrom(src => 1))
                  .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Order, OrderViewModel>()
                //.ForMember(des => des.OrderBoxDetails, opt => opt.MapFrom(src => src.OrderDetails.Select(x => x.BoxOrderDetails.Where(a => a.IsActive == true)).Select(a => a.)))
                //.ForMember(des => des.Status, opt => opt.MapFrom(src => src.Schedules.Any(x => x.Status == 1) ? 6 : src.Status))
                //.ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.OrderStorageDetails.Where(x => x.IsActive == true).FirstOrDefault().StorageId))
                //.ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.OrderStorageDetails.Where(x => x.IsActive == true).FirstOrDefault().Storage.Name))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));

            mc.CreateMap<Order, OrderByIdViewModel>()
                //.ForMember(des => des.OrderBoxDetails, opt => opt.MapFrom(src => src.OrderDetails.Select(x => x.BoxOrderDetails.Where(a => a.IsActive == true)).Select(a => a.)))
                //.ForMember(des => des.Status, opt => opt.MapFrom(src => src.Schedules.Any(x => x.Status == 1) ? 6 : src.Status))
                //.ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.OrderStorageDetails.Where(x => x.IsActive == true).FirstOrDefault().StorageId))
                //.ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.OrderStorageDetails.Where(x => x.IsActive == true).FirstOrDefault().Storage.Name))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate - src.DeliveryDate).Value.Days))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => ((src.ReturnDate - src.DeliveryDate).Value.Days) / 30));

            mc.CreateMap<OrderViewModel, Order>();

            mc.CreateMap<Order, OrderUpdateViewModel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            mc.CreateMap<OrderUpdateViewModel, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
