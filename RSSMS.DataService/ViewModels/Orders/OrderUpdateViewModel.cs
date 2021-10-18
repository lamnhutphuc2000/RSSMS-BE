using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderUpdateViewModel
    {
        public int Id { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string AddressReturn { get; set; }
        public int? Status { get; set; }
    }
}
