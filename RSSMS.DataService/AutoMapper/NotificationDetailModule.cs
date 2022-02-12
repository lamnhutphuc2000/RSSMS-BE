using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.NotificationDetails;

namespace RSSMS.DataService.AutoMapper
{
    public static class NotificationDetailModule
    {
        public static void ConfigNotificationDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<NotificationDetail, NotificationDetailViewModel>();
            mc.CreateMap<NotificationDetailViewModel, NotificationDetail>();
        }
    }
}
