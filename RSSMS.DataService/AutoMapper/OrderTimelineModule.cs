using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderTimelines;
using RSSMS.DataService.ViewModels.RequestTimelinesViewModel;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderTimelineModule
    {
        public static void ConfigOrderTimelineModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderTimeline, OrderTimelinesViewModel>()
                .ForMember(des => des.Datetime, opt => opt.MapFrom(src => src.Datetime));
            mc.CreateMap<RequestTimelinesViewModel, OrderTimelinesViewModel>()
                .ForMember(des => des.Datetime, opt => opt.MapFrom(src => src.Datetime));
        }
    }
}
