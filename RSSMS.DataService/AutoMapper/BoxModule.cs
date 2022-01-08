using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Boxes;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxModule
    {
        public static void ConfigBoxModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Box, BoxViewModel>()
                .ForMember(des => des.SizeType, opt => opt.MapFrom(des => des.Product.Size))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderBoxDetails.Where(x => x.IsActive == true).FirstOrDefault().OrderId));
            mc.CreateMap<Box, BoxDetailViewModel>()
                .ForMember(des => des.ShelfName, opt => opt.MapFrom(des => des.Shelf.Name))
                .ForMember(des => des.AreaName, opt => opt.MapFrom(des => des.Shelf.Area.Name))
                .ForMember(des => des.SizeType, opt => opt.MapFrom(des => des.Product.Size));
        }
    }
}
