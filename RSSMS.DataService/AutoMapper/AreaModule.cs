using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Areas;

namespace RSSMS.DataService.AutoMapper
{
    public static class AreaModule
    {
        public static void ConfigAreaModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Area, AreaViewModel>();
            mc.CreateMap<AreaViewModel, Area>();

            mc.CreateMap<AreaCreateViewModel, Area>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<AreaUpdateViewModel, Area>();

            mc.CreateMap<Area, AreaDetailViewModel>();
        }
    }
}
