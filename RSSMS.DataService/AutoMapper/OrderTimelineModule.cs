using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderTimelines;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderTimelineModule
    {
        public static void ConfigOrderTimelineModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderTimeline, OrderTimelinesViewModel>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(des => des.OrderId, opt => opt.MapFrom(src => src.Request.OrderId))
                .ForMember(des => des.Datetime, opt => opt.MapFrom(src => src.Datetime));
        }
    }
}
