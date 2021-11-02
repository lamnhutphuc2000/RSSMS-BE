using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Products;

namespace RSSMS.DataService.AutoMapper
{
    public static class ProductModule
    {
        public static void ConfigProductModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Product, ProductViewAllModel>();
            mc.CreateMap<ProductViewAllModel, Product>();
            
            mc.CreateMap<Product, ProductViewModel>();
            mc.CreateMap<ProductViewModel, Product>();

            mc.CreateMap<Product, ProductCreateViewModel>();
            mc.CreateMap<ProductCreateViewModel, Product>()
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Product, ProductUpdateViewModel>();
            mc.CreateMap<ProductUpdateViewModel, Product>()
                .ForMember(des => des.ModifiedDate, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
