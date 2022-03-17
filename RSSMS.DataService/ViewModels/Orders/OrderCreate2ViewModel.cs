using RSSMS.DataService.ViewModels.Services;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderCreate2ViewModel
    {
        public Guid? CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public string AddressReturn { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? TypeOrder { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public int? Duration { get; set; }


        public virtual ICollection<ServicesOrderViewModel> ListService { get; set; }


    }
}
