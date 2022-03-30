using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderDoneViewModel
    {
        public Guid OrderId { get; set; }
        public Guid RequestId { get; set; }
        public decimal? CompensationFee { get; set; }
        public string CompensationDescription { get; set; }
        public string AdditionalFeeDescription { get; set; }
        public double? AdditionalFee { get; set; }
    }
}
