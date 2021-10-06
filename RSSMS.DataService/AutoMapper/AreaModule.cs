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

            mc.CreateMap<Area, AreaCreateViewModel>();
            mc.CreateMap<AreaCreateViewModel, Area>()
                .ForMember(des => des.Usage, opt => opt.MapFrom(src => 0))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Area, AreaUpdateViewModel>();
            mc.CreateMap<AreaUpdateViewModel, Area>();
        }
    }
}
