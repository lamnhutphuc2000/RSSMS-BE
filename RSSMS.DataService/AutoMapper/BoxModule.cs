using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Boxes;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxModule
    {
        public static void ConfigBoxModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Box, BoxViewModel>();
        }
    }
}
