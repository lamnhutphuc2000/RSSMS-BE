using Microsoft.Extensions.DependencyInjection;
using RSSMS.DataService.DI;

namespace RSSMS.API.App_Start
{
    public static class DependencyInjectionResolver
    {
        public static void ConfigureDI(this IServiceCollection services)
        {
            services.ConfigServicesDI();
            services.ConfigureAutoMapper();
        }
    }
}
