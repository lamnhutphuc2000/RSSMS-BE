using RSSMS.DataService.ViewModels.BoxOrderDetails;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailByIdViewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal? Price { get; set; }
        public int? Amount { get; set; }
        public int? ProductType { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public string ServiceImageUrl { get; set; }

        //public BoxOrderViewModel BoxDetails { get; set; }
    }
}
