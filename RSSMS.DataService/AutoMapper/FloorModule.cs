using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Floors;

namespace RSSMS.DataService.AutoMapper
{
    public static class FloorModule
    {
        public static void ConfigFloorModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Floor, FloorInSpaceViewModel>()
                .ForMember(des => des.Usage, opt => opt.MapFrom(src => (double)0));

            mc.CreateMap<Floor, FloorGetByIdViewModel>()
                .ForMember(des => des.SpaceType, opt => opt.MapFrom(src => src.Space.Type))
                .ForMember(des => des.OrderDetails, opt => opt.Ignore());
        }
    }
}
