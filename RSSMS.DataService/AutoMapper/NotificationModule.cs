using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class NotificationModule
    {
        public static void ConfigNotificationModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Notification, NotificationViewModel>()
                .ForMember(des => des.IsOwn, opt => opt.MapFrom(src => src.NotificationDetails.FirstOrDefault().IsOwn))
                .ForMember(des => des.IsRead, opt => opt.MapFrom(src => src.NotificationDetails.FirstOrDefault().IsRead));
            mc.CreateMap<NotificationViewModel, Notification>();
        }
    }
}
