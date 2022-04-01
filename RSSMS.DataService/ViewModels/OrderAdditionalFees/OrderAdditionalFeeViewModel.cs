using System;

namespace RSSMS.DataService.ViewModels.OrderAdditionalFees
{
    public class OrderAdditionalFeeViewModel
    {
        public Guid Id { get; set; }
        public int? Type { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
    }
}
