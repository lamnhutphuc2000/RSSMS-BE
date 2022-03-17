using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailServiceViewModel
    {
        public Guid ServiceId { get; set; }
        public int Amount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
