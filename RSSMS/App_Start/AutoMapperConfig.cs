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
                mc.ConfigRolesModule();
                mc.ConfigAccountsModule();
                mc.ConfigUserModule();
                mc.ConfigImageModule();
                mc.ConfigStorageModule();
                mc.ConfigStaffManageOrderModule();
                mc.ConfigStaffAssignStoragesModule();
                mc.ConfigAreaModule();
                mc.ConfigOrderModule();
                mc.ConfigOrderTimelinesModule();
                mc.ConfigShelfModule();
                mc.ConfigFloorModule();
                mc.ConfigBoxModule();
                mc.ConfigOrderDetailModule();
                mc.ConfigOrderDetailServiceMapModule();
                mc.ConfigOrderStorageDetailModule();
                mc.ConfigOrderBoxDetailModule();
                mc.ConfigProductModule();
                mc.ConfigServicesModule();
                mc.ConfigRequestModule();
                mc.ConfigRequestDetailsModule();
                mc.ConfigScheduleModule();
                mc.ConfigBoxOrderDetailModule();
                mc.ConfigNotificationModule();
                mc.ConfigNotificationDetailModule();
                mc.ConfigOrderHistoryExtensionModule();
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
