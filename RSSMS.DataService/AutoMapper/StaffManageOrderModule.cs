using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.StaffManageUser;
using RSSMS.DataService.ViewModels.Users;

namespace RSSMS.DataService.AutoMapper
{
    public static class StaffManageOrderModule
    {
        public static void ConfigStaffManageOrderModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<StaffManageStorageViewModel, StaffManageStorage>();
            mc.CreateMap<StaffManageStorage, StaffManageStorageViewModel>();

            mc.CreateMap<StaffManageStorage, ManagerManageStorageViewModel>();

            mc.CreateMap<StaffManageStorageCreateViewModel, StaffManageStorage>();

            mc.CreateMap<StaffManageStorageUpdateViewModel, StaffManageStorage>();



        }
    }
}
