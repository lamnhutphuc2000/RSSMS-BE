using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Imports;

namespace RSSMS.DataService.AutoMapper
{
    public static class ImportModule
    {
        public static void ConfigImportModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Import, ImportViewModel>();


        }
    }
}
