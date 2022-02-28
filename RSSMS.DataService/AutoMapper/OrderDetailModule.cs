using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Products;
using RSSMS.DataService.ViewModels.Services;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailModule
    {
        public static void ConfigOrderDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetail, ServicesOrderViewModel>();
            mc.CreateMap<ServicesOrderViewModel, OrderDetail>();

            mc.CreateMap<OrderDetail, OrderDetailsViewModel>()
                .ForMember(des => des.ServiceImageUrl, opt => opt.MapFrom(src => src.Service.ImageUrl))
                .ForMember(des => des.Price, opt => opt.MapFrom(src => src.Service.Price));
            mc.CreateMap<OrderDetailsViewModel, OrderDetail>();

            mc.CreateMap<OrderDetail, OrderDetailByIdViewModel>()
                .ForMember(des => des.BoxDetails, opt => opt.MapFrom(src => src.Boxes))
                .ForMember(des => des.ProductImages, opt => opt.MapFrom(src => src.Service.ImageUrl))
                .ForMember(des => des.Price, opt => opt.MapFrom(src => src.Service.Price));
            mc.CreateMap<OrderDetailByIdViewModel, OrderDetail>();
        }
    }
}
