using RSSMS.DataService.ViewModels.OrderDetails;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderAssignFloorViewModel
    {
        public virtual ICollection<OrderDetailAssignFloorViewModel> OrderDetailAssignFloor { get; set; }
    }
}
