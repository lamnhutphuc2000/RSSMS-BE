﻿using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Shelves;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class ShelfModule
    {
        public static void ConfigShelfModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Shelf, ShelfViewModel>();
                //.ForMember(des => des.Boxes, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).OrderByDescending(x => x.CreatedDate)))
                //.ForMember(des => des.SizeType, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).FirstOrDefault().Service.Size))
                //.ForMember(des => des.ServiceId, opt => opt.MapFrom(src => src.Boxes.Where(x => x.IsActive == true).FirstOrDefault().ServiceId));

            mc.CreateMap<ShelfViewModel, Shelf>();

            mc.CreateMap<Shelf, ShelfCreateViewModel>();
            mc.CreateMap<ShelfCreateViewModel, Shelf>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Shelf, ShelfUpdateViewModel>();
            mc.CreateMap<ShelfUpdateViewModel, Shelf>();
        }
    }
}
