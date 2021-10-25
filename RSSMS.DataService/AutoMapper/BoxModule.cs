using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Boxes;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxModule
    {
        public static void ConfigBoxModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Box, BoxViewModel>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderBoxDetails.Where(x => x.IsActive == true).FirstOrDefault().OrderId));
        }
    }
}
