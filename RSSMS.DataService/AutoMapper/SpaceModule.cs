using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Spaces;

namespace RSSMS.DataService.AutoMapper
{
    public static class SpaceModule
    {
        public static void ConfigShelfModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Space, SpaceViewModel>();
            //.ForMember(des => des.Boxes, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).OrderByDescending(x => x.CreatedDate)))
            //.ForMember(des => des.SizeType, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).FirstOrDefault().Service.Size))
            //.ForMember(des => des.ServiceId, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).FirstOrDefault().ServiceId));

            mc.CreateMap<SpaceViewModel, Space>();

            mc.CreateMap<Space, SpaceCreateViewModel>();
            mc.CreateMap<SpaceCreateViewModel, Space>()
                .ForMember(des => des.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Space, SpaceUpdateViewModel>();
            mc.CreateMap<SpaceUpdateViewModel, Space>()
                .ForMember(des => des.Type, opt => opt.MapFrom(src => src.Type));
        }
    }
}
