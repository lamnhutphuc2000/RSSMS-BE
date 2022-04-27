using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Storages;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class StorageModule
    {
        public static void ConfigStorageModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Storage, StorageViewModel>()
                .ForMember(des => des.DeliveryFee, opt => opt.MapFrom(des => (decimal?)0))
                .ForMember(des => des.ManagerName, opt => opt.MapFrom(des => des.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager" && x.IsActive).FirstOrDefault() != null ? des.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager" && x.IsActive).FirstOrDefault().Staff.Name : null));
            mc.CreateMap<StorageViewModel, Storage>();


            mc.CreateMap<StorageCreateViewModel, Storage>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<StorageUpdateViewModel, Storage>();
            mc.CreateMap<Storage, StorageUpdateViewModel>();

            mc.CreateMap<Storage, StorageDetailViewModel>()
                .ForMember(des => des.ManagerName, opt => opt.MapFrom(des => des.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager" && x.IsActive == true).FirstOrDefault() != null ? des.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager").FirstOrDefault().Staff.Name : null));

            mc.CreateMap<Storage, StorageGetIdViewModel>();
            mc.CreateMap<StorageGetIdViewModel, Storage>();


        }
    }
}
