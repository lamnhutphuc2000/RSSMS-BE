using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderTimelines;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderTimelinesModule
    {
        public static void ConfigOrderTimelinesModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderTimeline, OrderTimelinesViewModel>();
        }
    }
}
