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
                mc.ConfigAccountModule();
                mc.ConfigAreaModule();
                mc.ConfigFloorModule();
                mc.ConfigImageModule();
                mc.ConfigNotificationModule();
                mc.ConfigOrderAdditionalFeeModule();
                mc.ConfigOrderDetailModule();
                mc.ConfigOrderDetailServiceMapModule();
                mc.ConfigOrderHistoryExtensionModule();
                mc.ConfigOrderModule();
                mc.ConfigOrderTimelineModule();
                mc.ConfigRequestDetailModule();
                mc.ConfigRequestModule();
                mc.ConfigRoleModule();
                mc.ConfigScheduleModule();
                mc.ConfigServiceModule();
                mc.ConfigSpaceModule();
                mc.ConfigStaffAssignStorageModule();
                mc.ConfigStorageModule();
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
