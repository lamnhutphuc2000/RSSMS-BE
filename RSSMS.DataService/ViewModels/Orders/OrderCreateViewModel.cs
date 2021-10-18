using System;
using System.Collections.Generic;
using System.Text;
using RSSMS.DataService.ViewModels.Products;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderCreateViewModel
    {
        public int? CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public string AddressReturn { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? TypeOrder { get; set; }
        public bool? IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? Duration { get; set; }


        public virtual ICollection<ProductOrderViewModel> ListProduct { get; set; }




    }
}
