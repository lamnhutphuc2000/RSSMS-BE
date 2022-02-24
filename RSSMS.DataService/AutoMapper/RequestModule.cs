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
            mc.CreateMap<Request, RequestViewModel>();
            //.ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.Order.OrderStorageDetails.Count > 0 ? (int?)src.Order.OrderStorageDetails.Where(x => x.IsActive == true).First().StorageId : null))
            //.ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.Order.OrderStorageDetails.Count > 0 ? src.Order.OrderStorageDetails.Where(x => x.IsActive == true).First().Storage.Name : null))
            //.ForMember(des => des.CustomerName, opt => opt.MapFrom(src => src.User.Role.Name == "Customer" ? src.User.Name : null))
            //.ForMember(des => des.CustomerPhone, opt => opt.MapFrom(src => src.User.Role.Name == "Customer" ? src.User.Name : null))
            //.ForMember(des => des.DeliveryStaffName, opt => opt.MapFrom(src => src.User.Role.Name == "Delivery Staff" ? src.User.Name : null))
            //.ForMember(des => des.DeliveryStaffPhone, otp => otp.MapFrom(src => src.User.Role.Name == "Delivery Staff" ? src.User.Phone : null));
            mc.CreateMap<RequestViewModel, Request>();

            mc.CreateMap<RequestCreateViewModel, Request>()
                .ForMember(des => des.IsActive, otp => otp.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Request, RequestUpdateViewModel>();
            mc.CreateMap<RequestUpdateViewModel, Request>();
            //.ForMember(des => des.ModifiedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<RequestByIdViewModel, Request>();
            mc.CreateMap<Request, RequestByIdViewModel>();
            //.ForMember(des => des.CancelBy, otp => otp.MapFrom(src => src.User.Name))
            //.ForMember(des => des.OldReturnDate, otp => otp.MapFrom(src => src.OrderHistoryExtensions.Count > 0 ? src.OrderHistoryExtensions.First().OldReturnDate : null))
            //.ForMember(des => des.OrderType, otp => otp.MapFrom(src => src.OrderHistoryExtensions.Count > 0 ? src.OrderHistoryExtensions.First().Order.TypeOrder : null))
            //.ForMember(des => des.ReturnDate, otp => otp.MapFrom(src => src.OrderHistoryExtensions.Count > 0 ? src.OrderHistoryExtensions.First().ReturnDate : src.ReturnDate))
            //.ForMember(des => des.TotalPrice, otp => otp.MapFrom(src => src.OrderHistoryExtensions.Count > 0 ? src.OrderHistoryExtensions.First().TotalPrice : null))
            //.ForMember(des => des.DurationMonths, otp => otp.MapFrom(src => src.OrderHistoryExtensions.Count > 0 ? (int?)(src.OrderHistoryExtensions.First().ReturnDate - src.OrderHistoryExtensions.First().OldReturnDate).Value.Days/30 : null))
            //.ForMember(des => des.CancelBy, otp => otp.MapFrom(src => src.User.Name))
            //.ForMember(des => des.CancelByPhone, otp => otp.MapFrom(src => src.User.Phone))
            //.ForMember(des => des.DurationDays, opt =>
            //{
            //    opt.PreCondition(src => src.OrderHistoryExtensions.Count > 0);
            //    opt.MapFrom(src => (src.OrderHistoryExtensions.First().ReturnDate - src.OrderHistoryExtensions.First().OldReturnDate).Value.Days);
            //});
        }
    }
}
