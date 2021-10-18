﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Products;

namespace RSSMS.DataService.ViewModels.Orders
{
    public partial class OrderViewModel
    {

        public static string[] Fields = {
            "Id","CustomerName","CustomerPhone","DeliveryAddress","AddressReturn","TotalPrice","TypeOrder"
                ,"IsUserDelivery","DeliveryDate","ReturnDate","Duration","Status"
        };
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public string? CustomerName { get; set; }
        [BindNever]
        public string? CustomerPhone{ get; set; }
        [BindNever]
        public string DeliveryAddress { get; set; }
        [BindNever]
        public string AddressReturn { get; set; }
        [BindNever]
        public decimal? TotalPrice { get; set; }
        [BindNever]
        public int? TypeOrder { get; set; }
        [BindNever]
        public bool? IsUserDelivery { get; set; }
        [BindNever]
        public DateTime? DeliveryDate { get; set; }
        [BindNever]
        public DateTime? ReturnDate { get; set; }
        [BindNever]
        public int? Duration { get; set; }
        [BindNever]
        public int? Status { get; set; }

        public virtual ICollection<OrderDetailsViewModel> OrderDetails { get; set; }
    }
}
