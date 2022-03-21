using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailServiceByIdViewModel
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceUrl { get; set; }
        public int Amount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
