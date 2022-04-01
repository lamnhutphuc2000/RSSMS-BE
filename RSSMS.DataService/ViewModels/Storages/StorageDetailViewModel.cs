using RSSMS.DataService.ViewModels.Orders;
using System;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageDetailViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ManagerName { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public int? Usage { get; set; }
        public OrderAreaViewModel OrderInfo { get; set; }
    }
}
