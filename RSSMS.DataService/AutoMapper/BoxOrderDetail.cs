using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.BoxOrderDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class BoxOrderDetailModule
    {
        public static void ConfigBoxOrderDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<BoxOrderDetailViewModel, BoxOrderDetail>();

            mc.CreateMap<BoxOrderDetailUpdateViewModel, BoxOrderDetail>()
                .ForMember(des => des.BoxId, opt => opt.MapFrom(src => src.NewBoxId))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
