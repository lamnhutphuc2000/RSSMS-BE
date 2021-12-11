using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public partial class OrderViewModel
    {

        public static string[] Fields = {
            "Id","CustomerName","CustomerPhone","DeliveryAddress","AddressReturn","TotalPrice","TypeOrder","RejectedReason"
                ,"IsUserDelivery","PaymentMethod","DeliveryTime","DeliveryDate","ReturnDate","DurationDays","DurationMonths","Status","IsPaid"
        };
        public int? Id { get; set; }
        [BindNever]
        public string CustomerName { get; set; }
        [BindNever]
        public string CustomerPhone { get; set; }
        [BindNever]
        public string DeliveryAddress { get; set; }
        [BindNever]
        public string AddressReturn { get; set; }
        [BindNever]
        public decimal? TotalPrice { get; set; }
        [BindNever]
        public string RejectedReason { get; set; }
        [BindNever]
        public int? TypeOrder { get; set; }
        [BindNever]
        public bool? IsUserDelivery { get; set; }
        [BindNever]
        public DateTime? DeliveryDate { get; set; }
        [BindNever]
        public string DeliveryTime { get; set; }
        [BindNever]
        public DateTime? ReturnDate { get; set; }
        [BindNever]
        public string ReturnTime { get; set; }
        [BindNever]
        public int? PaymentMethod { get; set; }
        [BindNever]
        public int? DurationDays { get; set; }
        [BindNever]
        public int? DurationMonths { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public bool? IsPaid { get; set; }


        public virtual ICollection<OrderDetailsViewModel> OrderDetails { get; set; }
    }
}
