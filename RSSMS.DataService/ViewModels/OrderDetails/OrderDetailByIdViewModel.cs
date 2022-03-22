using RSSMS.DataService.ViewModels.BoxOrderDetails;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailByIdViewModel
    {
        public Guid Id { get; set; }
        public Guid? FloorId { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public int? ServiceType { get; set; }
        public string ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<OrderDetailServiceByIdViewModel> OrderDetailServices { get; set; }
    }
}
