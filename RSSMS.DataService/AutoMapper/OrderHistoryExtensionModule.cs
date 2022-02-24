using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderHistoryExtension;
using RSSMS.DataService.ViewModels.Requests;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderHistoryExtensionModule
    {
        public static void ConfigOrderHistoryExtensionModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<RequestCreateViewModel, OrderHistoryExtension>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<OrderHistoryExtension, OrderHistoryExtensionViewModel>();
        }
    }
}
