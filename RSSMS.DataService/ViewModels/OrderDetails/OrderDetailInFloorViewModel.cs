using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailInFloorViewModel
    {
        public string OrderName { get; set; }
        public string CustomerName { get; set; }
        public int? OrderStatus { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public string StorageName { get; set; }
        public string AreaName { get; set; }
        public string SpaceName { get; set; }
        public string FloorName { get; set; }
        public Guid? ServiceId { get; set; }
        public int? ServiceType { get; set; }
        public string ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string ServiceImageUrl { get; set; }
        public ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<OrderDetailServiceByIdViewModel> OrderDetailServices { get; set; }
    }
}
