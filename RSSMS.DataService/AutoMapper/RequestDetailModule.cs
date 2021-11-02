using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.RequestDetails;

namespace RSSMS.DataService.AutoMapper
{
    public static class RequestDetailModule
    {
        public static void ConfigRequestDetailsModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<RequestDetail, RequestDetailCreateViewModel>();
            mc.CreateMap<RequestDetailCreateViewModel, RequestDetail>();

           
        }
    }
}
