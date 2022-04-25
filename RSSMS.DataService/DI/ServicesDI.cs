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
            services.AddScoped<IUtilService, UtilService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DbContext, RSSMSContext>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<IFloorRepository, FloorRepository>();
            services.AddScoped<IFloorService, FloorService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();

            services.AddScoped<IOrderHistoryExtensionRepository, OrderHistoryExtensionRepository>();
            services.AddScoped<IOrderHistoryExtensionService, OrderHistoryExtensionService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IOrderTimelineRepository, OrderTimelineRepository>();
            services.AddScoped<IOrderTimelineService, OrderTimelineService>();

            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestService, RequestService>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IScheduleService, ScheduleService>();

            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IServiceService, ServiceService>();

            services.AddScoped<ISpaceRepository, SpaceRepository>();
            services.AddScoped<ISpaceService, SpaceService>();

            services.AddScoped<IStaffAssignStorageRepository, StaffAssignStorageRepository>();
            services.AddScoped<IStaffAssignStorageService, StaffAssignStorageService>();

            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IStorageService, StorageService>();

            services.AddScoped<IRequestTimelineRepository, RequestTimelineRepository>();
            services.AddScoped<IRequestTimelineService, RequestTimelineService>();

            services.AddScoped<IFirebaseService, FirebaseService>();
        }
    }
}
