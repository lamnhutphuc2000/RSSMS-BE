using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Services;

namespace RSSMS.DataService.AutoMapper
{
    public static class ServicesModule
    {
        public static void ConfigServicesModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<Service, ServicesViewModel>();
            mc.CreateMap<ServicesViewModel, Service>();


            mc.CreateMap<Service, ServicesCreateViewModel>();
            mc.CreateMap<ServicesCreateViewModel, Service>()
                .ForMember(des => des.IsActive, opt => opt.MapFrom(src => true));

            mc.CreateMap<Service, ServicesUpdateViewModel>();
            mc.CreateMap<ServicesUpdateViewModel, Service>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
