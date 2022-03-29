using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Requests;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class RequestModule
    {
        public static void ConfigRequestModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Request, RequestViewModel>()
                .ForMember(des => des.OrderName, opt => opt.MapFrom(src => src.Order.Name))
                .ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.Storage != null ? src.StorageId : src.Order != null ? src.Order.StorageId : null))
                .ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.Storage != null ? src.Storage.Name : src.Order != null ? src.Order.Storage.Name : null))
                .ForMember(des => des.CustomerName, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
                .ForMember(des => des.CustomerPhone, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
                .ForMember(des => des.DeliveryStaffName, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Delivery Staff" ? src.CreatedByNavigation.Name : null))
                .ForMember(des => des.DeliveryStaffPhone, otp => otp.MapFrom(src => src.CreatedByNavigation.Role.Name == "Delivery Staff" ? src.CreatedByNavigation.Phone : null));
            mc.CreateMap<RequestViewModel, Request>();

            mc.CreateMap<RequestCreateViewModel, Request>()
                .ForMember(des => des.Status, opt => opt.MapFrom(src => 1))
                .ForMember(des => des.IsActive, otp => otp.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Request, RequestUpdateViewModel>();
            mc.CreateMap<RequestUpdateViewModel, Request>();
            //.ForMember(des => des.ModifiedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<RequestByIdViewModel, Request>();
            mc.CreateMap<Request, RequestByIdViewModel>()
                .ForMember(des => des.CustomerName, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
                .ForMember(des => des.CustomerPhone, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
                .ForMember(des => des.RequestDetails, otp => otp.MapFrom(src => src.RequestDetails))
            //.ForMember(des => des.OldReturnDate, otp => otp.MapFrom(src => src.Order.OrderHistoryExtensions.Count > 0 ? src.Order.OrderHistoryExtensions.Where.OldReturnDate : null))
            //.ForMember(des => des.OrderType, otp => otp.MapFrom(src => src.Order.OrderHistoryExtensions.Count > 0 ? src.Order.OrderHistoryExtensions.First().Order.TypeOrder : null))
            //.ForMember(des => des.ReturnDate, otp => otp.MapFrom(src => src.Order.OrderHistoryExtensions.Count > 0 ? src.Order.OrderHistoryExtensions.First().ReturnDate : src.ReturnDate))
            //.ForMember(des => des.TotalPrice, otp => otp.MapFrom(src => src.Order.OrderHistoryExtensions.Count > 0 ? src.Order.OrderHistoryExtensions.First().TotalPrice : null))
            //.ForMember(des => des.DurationMonths, otp => otp.MapFrom(src => src.Order.OrderHistoryExtensions.Count > 0 ? (int?)(src.Order.OrderHistoryExtensions.First().ReturnDate - src.OrderHistoryExtensions.First().OldReturnDate).Value.Days / 30 : null))
                .ForMember(des => des.CancelBy, otp => otp.MapFrom(src => src.Type == 3 ? src.CreatedByNavigation.Name : null))
                .ForMember(des => des.CancelByPhone, otp => otp.MapFrom(src => src.Type == 3 ? src.CreatedByNavigation.Phone : null))
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)(src.ReturnDate - src.DeliveryDate).Value.Days : null))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => (src.ReturnDate != null && src.DeliveryDate != null) ? (int?)((src.ReturnDate - src.DeliveryDate).Value.Days) / 30 : null));
            //.ForMember(des => des.DurationDays, opt =>
            //{
            //    opt.PreCondition(src => src.OrderHistoryExtensions.Count > 0);
            //    opt.MapFrom(src => (src.OrderHistoryExtensions.First().ReturnDate - src.OrderHistoryExtensions.First().OldReturnDate).Value.Days);
            //});

            mc.CreateMap<Request, RequestScheduleViewModel>()
               .ForMember(des => des.OrderName, opt => opt.MapFrom(src => src.Order.Name))
               .ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.Storage != null ? src.StorageId : src.Order != null ? src.Order.StorageId : null))
               .ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.Storage != null ? src.Storage.Name : src.Order != null ? src.Order.Storage.Name : null))
               .ForMember(des => des.CustomerName, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
                .ForMember(des => des.CustomerPhone, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Customer" ? src.CreatedByNavigation.Name : src.Customer.Name))
               .ForMember(des => des.DeliveryStaffName, opt => opt.MapFrom(src => src.CreatedByNavigation.Role.Name == "Delivery Staff" ? src.CreatedByNavigation.Name : null))
               .ForMember(des => des.DeliveryStaffPhone, otp => otp.MapFrom(src => src.CreatedByNavigation.Role.Name == "Delivery Staff" ? src.CreatedByNavigation.Phone : null));

        }
    }
}
