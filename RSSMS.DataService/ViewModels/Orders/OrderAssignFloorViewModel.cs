using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderAssignFloorViewModel
    {
        public Guid? DeliveryId { get; set; }
        public virtual ICollection<OrderDetailAssignFloorViewModel> OrderDetailAssignFloor { get; set; }
    }
}
