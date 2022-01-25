﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RSSMS.DataService.AutoMapper;

namespace RSSMS.API.App_Start
{
    public static class AutoMapperConfig
    {
        public static void ConfigureAutoMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.ConfigUserModule();
                mc.ConfigImageModule();
                mc.ConfigStorageModule();
                mc.ConfigStaffManageOrderModule();
                mc.ConfigAreaModule();
                mc.ConfigOrderModule();
                mc.ConfigShelfModule();
                mc.ConfigBoxModule();
                mc.ConfigOrderDetailModule();
                mc.ConfigOrderStorageDetailModule();
                mc.ConfigOrderBoxDetailModule();
                mc.ConfigProductModule();
                mc.ConfigRequestModule();
                mc.ConfigRequestDetailsModule();
                mc.ConfigScheduleModule();
                mc.ConfigBoxOrderDetailModule();
                mc.ConfigNotificationModule();
                mc.ConfigOrderHistoryExtensionModule();
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
