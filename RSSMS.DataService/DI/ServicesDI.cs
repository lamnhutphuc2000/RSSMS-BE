using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Services;
using RSSMS.DataService.UnitOfWorks;

namespace RSSMS.DataService.DI
{
    public static class ServicesDI
    {
        public static void ConfigServicesDI(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DbContext, RSSMSContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IStorageService, StorageService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IStaffManageStorageRepository, StaffManageStorageRepository>();
            services.AddScoped<IStaffManageStorageService, StaffManageStorageService>();

            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<IShelfRepository, ShelfRepository>();
            services.AddScoped<IShelfService, ShelfService>();
        }
    }
}
