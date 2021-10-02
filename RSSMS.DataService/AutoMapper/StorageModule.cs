using System;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Storages;

namespace RSSMS.DataService.AutoMapper
{
    public static class StorageModule
    {
        public static void ConfigStorageModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Storage, StorageViewModel>();
            mc.CreateMap<StorageViewModel, Storage>();


            mc.CreateMap<StorageCreateViewModel, Storage>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(des => des.Usage, opt => opt.MapFrom(src => 0))
                .ForMember(des => des.Type, opt => opt.MapFrom(src => 0));

            mc.CreateMap<StorageUpdateViewModel, Storage>()
                .ForMember(des => des.ModifiedDate, opt => opt.MapFrom(src => DateTime.Now));
            mc.CreateMap<Storage, StorageUpdateViewModel>();


            mc.CreateMap<Storage, StorageGetIdViewModel>();
            mc.CreateMap<StorageGetIdViewModel, Storage>();

        }
    }
}
