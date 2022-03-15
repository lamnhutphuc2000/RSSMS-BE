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

            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<IAccountsService, AccountsService>();

            services.AddScoped<IRolesRepository, RolesRepository>();
            services.AddScoped<IRolesService, RolesService>();

            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IStorageService, StorageService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IStaffManageStorageRepository, StaffManageStorageRepository>();
            services.AddScoped<IStaffManageStorageService, StaffManageStorageService>();

            services.AddScoped<IStaffAssignStoragesRepository, StaffAssignStoragesRepository>();
            services.AddScoped<IStaffAssignStoragesService, StaffAssignStoragesService>();

            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<ISpaceRepository, SpaceRepository>();
            services.AddScoped<ISpaceService, SpaceService>();

            services.AddScoped<IFloorsRepository, FloorsRepository>();
            services.AddScoped<IFloorsService, FloorsService>();

            services.AddScoped<IBoxRepository, BoxRepository>();
            services.AddScoped<IBoxService, BoxService>();

            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();

            services.AddScoped<IOrderTimelinesRepository, OrderTimelinesRepository>();
            services.AddScoped<IOrderTimelinesService, OrderTimelinesService>();

            services.AddScoped<IOrderBoxDetailRepository, OrderBoxDetailRepository>();
            services.AddScoped<IOrderBoxDetailService, OrderBoxDetailService>();

            services.AddScoped<IOrderStorageDetailRepository, OrderStorageDetailRepository>();
            services.AddScoped<IOrderStorageDetailService, OrderStorageDetailService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<IServicesRepository, ServicesRepository>();
            services.AddScoped<IServicesService, ServicesService>();

            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestService, RequestService>();

            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IScheduleService, ScheduleService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<INotificationDetailRepository, NotificationDetailRepository>();
            services.AddScoped<INotificationDetailService, NotificationDetailService>();

            //services.AddScoped<IBoxOrderDetailRepository, BoxOrderDetailRepository>();
            //services.AddScoped<IBoxOrderDetailService, BoxOrderDetailService>();

            services.AddScoped<IOrderHistoryExtensionRepository, OrderHistoryExtensionRepository>();
            services.AddScoped<IOrderHistoryExtensionService, OrderHistoryExtensionService>();

            services.AddScoped<IFirebaseService, FirebaseService>();
        }
    }
}
