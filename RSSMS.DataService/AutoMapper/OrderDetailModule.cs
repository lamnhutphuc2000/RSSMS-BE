using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Products;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailModule
    {
        public static void ConfigOrderDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetail, ProductOrderViewModel>();
            mc.CreateMap<ProductOrderViewModel, OrderDetail>();

            mc.CreateMap<OrderDetail, OrderDetailsViewModel>()
                .ForMember(des => des.BoxDetails, opt => opt.MapFrom(src => src.BoxOrderDetails))
                .ForMember(des => des.ProductImages, opt => opt.MapFrom(src => src.Product.Images))
                .ForMember(des => des.Price, opt => opt.MapFrom(src => src.Product.Price));
            mc.CreateMap<OrderDetailsViewModel, OrderDetail>();
        }
    }
}
