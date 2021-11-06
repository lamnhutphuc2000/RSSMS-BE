using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Shelves;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class ShelfModule
    {
        public static void ConfigShelfModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Shelf, ShelfViewModel>()
                .ForMember(des => des.ProductId, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).FirstOrDefault().ProductId));

            mc.CreateMap<ShelfViewModel, Shelf>();

            mc.CreateMap<Shelf, ShelfCreateViewModel>();
            mc.CreateMap<ShelfCreateViewModel, Shelf>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Shelf, ShelfUpdateViewModel>();
            mc.CreateMap<ShelfUpdateViewModel, Shelf>();
        }
    }
}
