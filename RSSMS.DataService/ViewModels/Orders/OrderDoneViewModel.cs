using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderAdditionalFees;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderDoneViewModel
    {
        public Guid OrderId { get; set; }
        public Guid RequestId { get; set; }
        public decimal? CompensationFee { get; set; }
        public string CompensationDescription { get; set; }
        public virtual ICollection<OrderAdditionalFeeCreateViewModel> OrderAdditionalFees { get; set; }
    }
}
