using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Orders;
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

            mc.CreateMap<Order, OrderStorageViewModel>()
                .ForMember(des => des.DurationDays, opt => opt.MapFrom(src => (src.ReturnDate - src.DeliveryDate).Value.Days))
                .ForMember(des => des.DurationMonths, opt => opt.MapFrom(src => ((src.ReturnDate - src.DeliveryDate).Value.Days) / 30));
            mc.CreateMap<OrderStorageViewModel, Order>();
        }
    }
}
