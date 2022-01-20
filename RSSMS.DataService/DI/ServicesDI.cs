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

            services.AddScoped<IBoxRepository, BoxRepository>();
            services.AddScoped<IBoxService, BoxService>();

            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();

            services.AddScoped<IOrderBoxDetailRepository, OrderBoxDetailRepository>();
            services.AddScoped<IOrderBoxDetailService, OrderBoxDetailService>();

            services.AddScoped<IOrderStorageDetailRepository, OrderStorageDetailRepository>();
            services.AddScoped<IOrderStorageDetailService, OrderStorageDetailService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestService, RequestService>();

            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IScheduleService, ScheduleService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<INotificationDetailRepository, NotificationDetailRepository>();
            services.AddScoped<INotificationDetailService, NotificationDetailService>();

            services.AddScoped<IFirebaseService, FirebaseService>();
        }
    }
}
