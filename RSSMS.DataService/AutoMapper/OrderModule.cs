using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Orders;
using System;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderModule
    {
        public static void ConfigOrderModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderCreateViewModel, Order>()
                .ForMember(des => des.ReturnTime, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.ReturnTime) ? (TimeSpan?)TimeUtilStatic.StringToTime(src.ReturnTime) : null))
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.DeliveryTime) ? (TimeSpan?)TimeUtilStatic.StringToTime(src.DeliveryTime) : null))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => 1))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Order, OrderViewModel>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => src.DeliveryTime != null ? TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime) : ""))
                .ForMember(des => des.Status, opt => opt.MapFrom(src => src.Status > 4 ? src.Status : (src.ReturnDate - DateTime.Now).Value.Days < 0 ? 4 : (src.ReturnDate - DateTime.Now).Value.Days < 3 ? 3 : src.Status))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));

            mc.CreateMap<Order, OrderByIdViewModel>()
                .ForMember(des => des.ExportStaff, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export.CreatedByNavigation.Name : null))
                .ForMember(des => des.ExportDay, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export.CreatedDate : null))
                .ForMember(des => des.ExportDeliveryBy, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export.DeliveryByNavigation.Name : null))
                .ForMember(des => des.ExportReturnAddress, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export.ReturnAddress : null))
                .ForMember(des => des.ExportCode, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Export.Code : null))
                .ForMember(des => des.StorageAddress, opt => opt.MapFrom(src => src.Storage.Address))
                .ForMember(des => des.ImportStaff, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import.CreatedByNavigation.Name : null))
                .ForMember(des => des.ImportDay, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import.CreatedDate : null))
                .ForMember(des => des.ImportDeliveryBy, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import.DeliveryByNavigation.Name : null))
                .ForMember(des => des.ImportCode, opt => opt.MapFrom(src => src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import != null ? src.OrderDetails.Where(orderDetail => orderDetail.Height != 0 && orderDetail.Width != 0 && orderDetail.Length != 0).FirstOrDefault().Import.Code : null))
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => src.DeliveryTime != null ? TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime) : ""))
                .ForMember(des => des.Status, opt => opt.MapFrom(src => src.Status > 4 ? src.Status : (src.ReturnDate - DateTime.Now).Value.Days < 0 ? 4 : (src.ReturnDate - DateTime.Now).Value.Days < 3 ? 3 : src.Status))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));


            mc.CreateMap<Order, OrderUpdateViewModel>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => src.DeliveryTime != null ? TimeUtilStatic.TimeToString((TimeSpan)src.DeliveryTime) : ""))
                .ForMember(des => des.ReturnTime, opt => opt.MapFrom(src => src.ReturnTime != null ? TimeUtilStatic.TimeToString((TimeSpan)src.ReturnTime) : ""))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            mc.CreateMap<OrderUpdateViewModel, Order>()
                .ForMember(des => des.DeliveryTime, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.DeliveryTime) ? (TimeSpan?)TimeUtilStatic.StringToTime(src.DeliveryTime) : null))
                .ForMember(des => des.ReturnTime, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.ReturnTime) ? (TimeSpan?)TimeUtilStatic.StringToTime(src.ReturnTime) : null))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            mc.CreateMap<OrderCreateViewModel, OrderByIdViewModel>()
                ;

        }
    }
}
