using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Boxes;
using RSSMS.DataService.ViewModels.BoxOrderDetails;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxModule
    {
        public static void ConfigBoxModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Box, BoxViewModel>()
                .ForMember(dest => dest.ReturnDate,opt => opt.MapFrom(src => src.OrderDetail.Order.ReturnDate))
                .ForMember(des => des.SizeType, opt => opt.MapFrom(des => des.Service.Name))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderDetailId));

            mc.CreateMap<Box, BoxOrderViewModel>()
                .ForMember(des => des.ShelfName, opt => opt.MapFrom(des => des.Shelf.Name))
                .ForMember(des => des.AreaId, opt => opt.MapFrom(des => des.Shelf.Area.Id))
                .ForMember(des => des.AreaName, opt => opt.MapFrom(des => des.Shelf.Area.Name));
        }
    }
}
