using AutoMapper;
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
                mc.ConfigImageModule();
                mc.ConfigStorageModule();
                mc.ConfigStaffAssignStoragesModule();
                mc.ConfigAreaModule();
                mc.ConfigOrderModule();
                mc.ConfigOrderTimelinesModule();
                mc.ConfigShelfModule();
                mc.ConfigFloorModule();
                mc.ConfigOrderDetailModule();
                mc.ConfigOrderDetailServiceMapModule();
                mc.ConfigServicesModule();
                mc.ConfigRequestModule();
                mc.ConfigRequestDetailsModule();
                mc.ConfigScheduleModule();
                mc.ConfigNotificationModule();
                mc.ConfigOrderHistoryExtensionModule();
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
