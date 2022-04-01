using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.RequestDetail;

namespace RSSMS.DataService.AutoMapper
{
    public static class RequestDetailModule
    {
        public static void ConfigRequestDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<RequestDetailCreateViewModel, RequestDetail>();

            mc.CreateMap<RequestDetail, RequestDetailViewModel>()
                .ForMember(des => des.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.Service.Type))
                .ForMember(des => des.ServiceImageUrl, opt => opt.MapFrom(src => src.Service.ImageUrl));
        }
    }
}
