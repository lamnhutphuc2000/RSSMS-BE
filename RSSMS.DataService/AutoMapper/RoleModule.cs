using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Roles;

namespace RSSMS.DataService.AutoMapper
{
    public static class RoleModule
    {
        public static void ConfigRoleModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Role, RolesViewModel>();


        }
    }
}
