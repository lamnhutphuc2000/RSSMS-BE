using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderDetails;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailServiceMapModule
    {
        public static void ConfigOrderDetailServiceMapModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetailServiceViewModel, OrderDetailServiceMap>()
                .ForMember(des => des.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(des => des.Price, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(des => des.ServiceId, opt => opt.MapFrom(src => src.ServiceId));

            mc.CreateMap<OrderDetailServiceViewModel, OrderDetailServiceByIdViewModel>();

            mc.CreateMap<OrderDetailServiceMap, OrderDetailServiceByIdViewModel>()
                .ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.Service.Type))
                .ForMember(des => des.ServiceUrl, opt => opt.MapFrom(src => src.Service.ImageUrl));
        }
    }
}
