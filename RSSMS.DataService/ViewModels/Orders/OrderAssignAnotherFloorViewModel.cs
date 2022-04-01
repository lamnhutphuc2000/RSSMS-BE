using RSSMS.DataService.ViewModels.OrderDetails;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderAssignAnotherFloorViewModel
    {
        public virtual ICollection<OrderDetailAssignAnotherFloorViewModel> OrderDetailAssignFloor { get; set; }
    }
}
