using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.AutoMapper
{
    public static class ImportModule
    {
        public static void ConfigImportModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Import, ImportViewModel>();


        }
    }
}
