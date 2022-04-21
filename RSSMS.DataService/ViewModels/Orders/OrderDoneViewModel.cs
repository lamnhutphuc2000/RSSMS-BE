using RSSMS.DataService.ViewModels.OrderAdditionalFees;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderDoneViewModel
    {
        public Guid OrderId { get; set; }
        public Guid RequestId { get; set; }
        public int? Status { get; set; }
        public Guid? DeliveryBy { get; set; }
        public string ReturnAddress { get; set; }
        public virtual ICollection<OrderAdditionalFeeCreateViewModel> OrderAdditionalFees { get; set; }
    }
}
