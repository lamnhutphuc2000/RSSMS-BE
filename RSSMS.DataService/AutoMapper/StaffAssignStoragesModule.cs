using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class StaffAssignStoragesModule
    {
        public static void ConfigStaffAssignStoragesModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<StaffAssignStorageViewModel, StaffAssignStorage>();
            mc.CreateMap<StaffAssignStorage, StaffAssignStorageViewModel>();


            mc.CreateMap<StaffAssignStorageCreateViewModel, StaffAssignStorage>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(des => des.StaffId, opt => opt.MapFrom(src => src.UserId));
        }
    }
}
