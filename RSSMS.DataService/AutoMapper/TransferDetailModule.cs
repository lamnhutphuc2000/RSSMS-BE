using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.TransferDetails;

namespace RSSMS.DataService.AutoMapper
{
    public static class TransferDetailModule
    {
        public static void ConfigTransferDetailModule(this IMapperConfigurationExpression mc)
        {
            mc.CreateMap<TransferDetail, TransferDetailViewModel>()
                .ForMember(des => des.AreaFromName, opt => opt.MapFrom(src => src.Transfer.FloorFrom.Space.Area.Name))
                .ForMember(des => des.SpaceFromName, opt => opt.MapFrom(src => src.Transfer.FloorFrom.Space.Name))
                .ForMember(des => des.FloorFromName, opt => opt.MapFrom(src => src.Transfer.FloorFrom.Name))
                .ForMember(des => des.AreaToName, opt => opt.MapFrom(src => src.Transfer.FloorTo.Space.Area.Name))
                .ForMember(des => des.SpaceToName, opt => opt.MapFrom(src => src.Transfer.FloorTo.Space.Name))
                .ForMember(des => des.FloorToName, opt => opt.MapFrom(src => src.Transfer.FloorTo.Name))
                .ForMember(des => des.CreatedDate, opt => opt.MapFrom(src => src.Transfer.CreatedDate))
                .ForMember(des => des.StaffName, opt => opt.MapFrom(src => src.Transfer.CreatedByNavigation.Name));

        }
    }
}
