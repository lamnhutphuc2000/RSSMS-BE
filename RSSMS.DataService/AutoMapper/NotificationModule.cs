using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Notifications;

namespace RSSMS.DataService.AutoMapper
{
    public static class NotificationModule
    {
        public static void ConfigNotificationModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Notification, NotificationViewModel>()
                .ForMember(des => des.CreateDate , opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(des => des.IsRead, opt => opt.MapFrom(src => src.IsRead));
        }
    }
}
