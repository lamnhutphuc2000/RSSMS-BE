using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderStorages;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderStorageDetailModule
    {
        public static void ConfigOrderStorageDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderStorageDetail, OrderStorageDetailViewModel>();
            mc.CreateMap<OrderStorageDetailViewModel, OrderStorageDetail>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
