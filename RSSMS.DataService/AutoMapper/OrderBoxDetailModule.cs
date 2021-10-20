using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderBoxes;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderBoxDetailModuleModule
    {
        public static void ConfigOrderBoxDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderBoxDetail, OrderBoxesDetailViewModel>();
            mc.CreateMap<OrderBoxesDetailViewModel, OrderBoxDetail>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<OrderBoxDetail, OrderBoxesMoveViewModel>();
            mc.CreateMap<OrderBoxesMoveViewModel, OrderBoxDetail>()
                .ForMember(des => des.BoxId, opt => opt.MapFrom(src => src.NewBoxId))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
