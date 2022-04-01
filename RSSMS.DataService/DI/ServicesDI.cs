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

            services.AddScoped<IAccountsRepository, AccountRepository>();
            services.AddScoped<IAccountsService, AccountService>();

            services.AddScoped<IRolesRepository, RoleRepository>();
            services.AddScoped<IRolesService, RoleService>();

            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IStorageService, StorageService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IStaffAssignStoragesRepository, StaffAssignStorageRepository>();
            services.AddScoped<IStaffAssignStoragesService, StaffAssignStorageService>();

            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<ISpaceRepository, SpaceRepository>();
            services.AddScoped<ISpaceService, SpaceService>();

            services.AddScoped<IFloorsRepository, FloorRepository>();
            services.AddScoped<IFloorsService, FloorService>();

            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();

            services.AddScoped<IOrderTimelinesRepository, OrderTimelineRepository>();
            services.AddScoped<IOrderTimelinesService, OrderTimelineService>();

            services.AddScoped<IServicesRepository, ServiceRepository>();
            services.AddScoped<IServicesService, ServiceService>();

            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestService, RequestService>();

            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IScheduleService, ScheduleService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<IOrderHistoryExtensionRepository, OrderHistoryExtensionRepository>();
            services.AddScoped<IOrderHistoryExtensionService, OrderHistoryExtensionService>();

            services.AddScoped<IFirebaseService, FirebaseService>();
        }
    }
}
