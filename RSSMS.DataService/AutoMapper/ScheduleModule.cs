using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Schedules;

namespace RSSMS.DataService.AutoMapper
{
    public static class ScheduleModule
    {
        public static void ConfigScheduleModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Schedule, ScheduleViewModel>()
                .ForMember(des => des.ScheduleTime, opt => opt.MapFrom(src => TimeUtilStatic.TimeToString(src.ScheduleTime)))
                .ForMember(des => des.ScheduleDay, opt => opt.MapFrom(src => src.ScheduleDay));

            mc.CreateMap<Schedule, ScheduleOrderViewModel>();
            mc.CreateMap<ScheduleOrderViewModel, Schedule>()
                .ForMember(des => des.ScheduleTime, opt => opt.MapFrom(src => TimeUtilStatic.StringToTime(src.DeliveryTime)))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Schedule, ScheduleCreateViewModel>();
            mc.CreateMap<ScheduleCreateViewModel, Schedule>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
