using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.AutoMapper
{
    public static class ImageModule
    {
        public static void ConfigImageModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Image, AvatarImageViewModel>();
            mc.CreateMap<AvatarImageViewModel, Image>();

            mc.CreateMap<AvatarImageCreateViewModel, Image>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
