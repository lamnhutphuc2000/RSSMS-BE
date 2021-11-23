using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Schedules;

namespace RSSMS.DataService.AutoMapper
{
    public static class ScheduleModule
    {
        public static void ConfigScheduleModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Schedule, ScheduleOrderViewModel>();
            mc.CreateMap<ScheduleOrderViewModel, Schedule>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
