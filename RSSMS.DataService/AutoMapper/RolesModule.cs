﻿using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class RolesModule
    {
        public static void ConfigRolesModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Role, RolesViewModel>();


        }
    }
}
