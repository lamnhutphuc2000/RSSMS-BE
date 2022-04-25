using System;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailServiceViewModel
    {
        public Guid ServiceId { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}
