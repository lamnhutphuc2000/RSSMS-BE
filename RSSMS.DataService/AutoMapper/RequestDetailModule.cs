using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.RequestDetail;

namespace RSSMS.DataService.AutoMapper
{
    public static class RequestDetailModule
    {
        public static void ConfigRequestDetailsModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<RequestDetailCreateViewModel, RequestDetail>();
        }
    }
}
