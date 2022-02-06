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
                .ForMember(des => des.DeliveryStaffName, opt => opt.MapFrom(src => src.User.Role.Name == "Delivery Staff" ? src.User.Name : null))
                .ForMember(des => des.DeliveryStaffPhone, otp => otp.MapFrom(src => src.User.Role.Name == "Delivery Staff" ? src.User.Phone : null));
            mc.CreateMap<RequestViewModel, Request>();

            mc.CreateMap<RequestCreateViewModel, Request>()
                .ForMember(des => des.IsActive, otp => otp.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Request, RequestUpdateViewModel>();
            mc.CreateMap<RequestUpdateViewModel, Request>()
                .ForMember(des => des.ModifiedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<RequestByIdViewModel, Request>();
            mc.CreateMap<Request, RequestByIdViewModel>()
                .ForMember(des => des.CancelBy, otp => otp.MapFrom(src => src.User.Name))
                .ForMember(des => des.CancelByPhone, otp => otp.MapFrom(src => src.User.Phone));
        }
    }
}
