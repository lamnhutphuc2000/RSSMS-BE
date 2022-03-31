using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderAssignAnotherFloorViewModel
    {
        public virtual ICollection<OrderDetailAssignAnotherFloorViewModel> OrderDetailAssignFloor { get; set; }
    }
}
