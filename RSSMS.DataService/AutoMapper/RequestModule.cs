﻿using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Requests;

namespace RSSMS.DataService.AutoMapper
{
    public static class RequestModule
    {
        public static void ConfigRequestModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Request, RequestViewModel>();
            mc.CreateMap<RequestViewModel, Request>();

            mc.CreateMap<Request, RequestCreateViewModel>();
            mc.CreateMap<RequestCreateViewModel, Request>()
                .ForMember(des => des.IsActive, otp => otp.MapFrom(src => true))
                .ForMember(des => des.CreatedDate, otp => otp.MapFrom(src => DateTime.Now));

            mc.CreateMap<Request, RequestUpdateViewModel>();
            mc.CreateMap<RequestUpdateViewModel, Request>()
                .ForMember(des => des.ModifiedDate, otp => otp.MapFrom(src => DateTime.Now));
        }
    }
}