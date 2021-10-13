using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Shelves;

namespace RSSMS.DataService.AutoMapper
{
    public static class ShelfModule
    {
        public static void ConfigShelfModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Shelf, ShelfViewModel>();
            mc.CreateMap<ShelfViewModel, Shelf>();

            mc.CreateMap<Shelf, ShelfCreateViewModel>();
            mc.CreateMap<ShelfCreateViewModel, Shelf>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Shelf, ShelfUpdateViewModel>();
            mc.CreateMap<ShelfUpdateViewModel, Shelf>();
        }
    }
}
