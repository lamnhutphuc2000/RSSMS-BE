using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Schedules;
using System;

namespace RSSMS.DataService.AutoMapper
{
    public static class ScheduleModule
    {
        public static void ConfigScheduleModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Schedule, ScheduleViewModel>()
                .ForMember(des => des.ScheduleTime, opt => opt.MapFrom(src => src.ScheduleTime != null ? TimeUtilStatic.TimeToString((TimeSpan)src.ScheduleTime) : ""))
                .ForMember(des => des.ScheduleDay, opt => opt.MapFrom(src => src.ScheduleDay));

            mc.CreateMap<Schedule, ScheduleOrderViewModel>();
            mc.CreateMap<ScheduleOrderViewModel, Schedule>()
                .ForMember(des => des.ScheduleTime, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.DeliveryTime) ? (TimeSpan?)TimeUtilStatic.StringToTime(src.DeliveryTime) : null))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Schedule, ScheduleCreateViewModel>();
            mc.CreateMap<ScheduleCreateViewModel, Schedule>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
