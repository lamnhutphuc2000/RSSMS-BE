using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Services;
using System;
using System.Linq;

namespace RSSMS.DataService.AutoMapper
{
    public static class OrderDetailModule
    {
        public static void ConfigOrderDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<OrderDetail, ServicesOrderViewModel>();
            mc.CreateMap<ServicesOrderViewModel, OrderDetail>();

            mc.CreateMap<OrderDetailViewModel, OrderDetail>()
                .ForMember(des => des.OrderDetailServiceMaps, opt => opt.MapFrom(src => src.OrderDetailServices));

            mc.CreateMap<OrderDetailViewModel, OrderDetailByIdViewModel>();


            mc.CreateMap<OrderDetail, OrderDetailByIdViewModel>()
                .ForMember(des => des.FloorName, opt => opt.MapFrom(src => src.Floor.Name))
                .ForMember(des => des.SpaceName, opt => opt.MapFrom(src => src.Floor.Space.Name))
                .ForMember(des => des.AreaName, opt => opt.MapFrom(src => src.Floor.Space.Area.Name))
                .ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.Floor.Space.Area.Storage.Name))
                .ForMember(des => des.ServiceId, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Id : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Id))
                .ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Type : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Type))
                .ForMember(des => des.ServiceName, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Name : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Name))
                .ForMember(des => des.ServicePrice, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Price : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Price))
                .ForMember(des => des.ServiceImageUrl, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.ImageUrl : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.ImageUrl))
                .ForMember(des => des.OrderDetailServices, opt => opt.MapFrom(src => src.OrderDetailServiceMaps));
            //.ForMember(des => des.BoxDetails, opt => opt.MapFrom(src => src.Boxes))
            //.ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.Service.Type))
            //.ForMember(des => des.ServiceImageUrl, opt => opt.MapFrom(src => src.Service.ImageUrl))
            //.ForMember(des => des.Price, opt => opt.MapFrom(src => src.Service.Price));
            mc.CreateMap<OrderDetailByIdViewModel, OrderDetail>();

            mc.CreateMap<OrderDetail, OrderDetailInFloorViewModel>()
                .ForMember(des => des.OrderStatus, opt => opt.MapFrom(src => (src.Order.ReturnDate - DateTime.Now).Value.Days > 0 ? (int?)(src.Order.ReturnDate - DateTime.Now).Value.Days <= 3 ? 3 : src.Order.Status : 4))
                .ForMember(des => des.FloorName, opt => opt.MapFrom(src => src.Floor.Name))
                .ForMember(des => des.SpaceName, opt => opt.MapFrom(src => src.Floor.Space.Name))
                .ForMember(des => des.AreaName, opt => opt.MapFrom(src => src.Floor.Space.Area.Name))
                .ForMember(des => des.StorageName, opt => opt.MapFrom(src => src.Floor.Space.Area.Storage.Name))
                .ForMember(des => des.ServiceId, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Id : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Id))
                .ForMember(des => des.ServiceType, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Type : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Type))
                .ForMember(des => des.ServiceName, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Name : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Name))
                .ForMember(des => des.ServicePrice, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.Price : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.Price))
                .ForMember(des => des.ServiceImageUrl, opt => opt.MapFrom(src => src.OrderDetailServiceMaps.Count == 1 ? src.OrderDetailServiceMaps.First().Service.ImageUrl : src.OrderDetailServiceMaps.Where(x => x.Service.Type == 3 || x.Service.Type == 2).First().Service.ImageUrl))
                .ForMember(des => des.CustomerName, opt => opt.MapFrom(src => src.Order.Customer.Name))
                .ForMember(des => des.OrderName, opt => opt.MapFrom(src => src.Order.Name))
                .ForMember(des => des.OrderStatus, opt => opt.MapFrom(src => src.Order.Status))
                .ForMember(des => des.ReturnDate, opt => opt.MapFrom(src => src.Order.ReturnDate))
                .ForMember(des => des.OrderDetailServices, opt => opt.MapFrom(src => src.OrderDetailServiceMaps));
        }
    }
}
