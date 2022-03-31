using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailAssignAnotherFloorViewModel
    {
        public Guid OrderDetailId { get; set; }
        public Guid FloorId { get; set; }
        public Guid OldFloorId { get; set; }
    }
}
