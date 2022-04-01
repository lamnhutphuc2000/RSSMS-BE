using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderAdditionalFees
{
    public class OrderAdditionalFeeCreateViewModel
    {
        public int? Type { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
    }
}
