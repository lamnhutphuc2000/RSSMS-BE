using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.StaffManageUser;
using RSSMS.DataService.ViewModels.Storages;
using System;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class StorageModule
    {
        public static void ConfigStorageModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Storage, StorageViewModel>()
                 .ForMember(des => des.ManagerName, opt => opt.MapFrom(des => des.StaffManageStorages.FirstOrDefault().User.Name)); ;
            mc.CreateMap<StorageViewModel, Storage>();


            mc.CreateMap<StorageCreateViewModel, Storage>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(des => des.Usage, opt => opt.MapFrom(src => 0));

            mc.CreateMap<StorageUpdateViewModel, Storage>()
                .ForMember(des => des.ModifiedDate, opt => opt.MapFrom(src => DateTime.Now));
            mc.CreateMap<Storage, StorageUpdateViewModel>();


            mc.CreateMap<Storage, StorageGetIdViewModel>()
                .ForMember(des => des.ManagerName, opt => opt.MapFrom(des => des.StaffManageStorages.FirstOrDefault().User.Name));
            mc.CreateMap<StorageGetIdViewModel, Storage>();

            mc.CreateMap<Storage, StaffManageStorageCreateViewModel>()
                .ForMember(des => des.StorageId, opt => opt.MapFrom(src => src.Id));

        }
    }
}
