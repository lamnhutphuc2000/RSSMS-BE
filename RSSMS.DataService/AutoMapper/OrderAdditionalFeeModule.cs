using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderAdditionalFees;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderAdditionalFeeModule
    {
        public static void ConfigOrderAdditionalFeeModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderAdditionalFee, OrderAdditionalFeeViewModel>();


            mc.CreateMap<OrderAdditionalFeeCreateViewModel, OrderAdditionalFee>();

        }
    }
}
