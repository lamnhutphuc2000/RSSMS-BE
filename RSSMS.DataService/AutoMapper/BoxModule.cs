using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Boxes;
using RSSMS.DataService.ViewModels.BoxOrderDetails;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxModule
    {
        public static void ConfigBoxModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Box, BoxViewModel>()
                .ForMember(dest => dest.ReturnDate,
                   opt => opt.MapFrom
                   (src => src.BoxOrderDetails.Where(a => a.IsActive == true).FirstOrDefault() != null ? src.BoxOrderDetails.Where(a => a.IsActive == true).FirstOrDefault().OrderDetail.Order.ReturnDate : null))
                .ForMember(des => des.SizeType, opt => opt.MapFrom(des => des.Product.Size))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.BoxOrderDetails.Where(x => x.IsActive == true).FirstOrDefault().OrderDetail.OrderId));
            
            mc.CreateMap<Box, BoxOrderViewModel>()
                .ForMember(des => des.ShelfName, opt => opt.MapFrom(des => des.Shelf.Name))
                .ForMember(des => des.AreaId, opt => opt.MapFrom(des => des.Shelf.Area.Id))
                .ForMember(des => des.AreaName, opt => opt.MapFrom(des => des.Shelf.Area.Name));
        }
    }
}
