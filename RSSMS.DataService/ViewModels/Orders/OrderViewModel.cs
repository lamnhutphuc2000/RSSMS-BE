using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.OrderAdditionalFees;
using RSSMS.DataService.ViewModels.OrderHistoryExtension;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public partial class OrderViewModel
    {

        public static string[] Fields = {
            "Id","CustomerName","Name","CustomerPhone","DeliveryAddress","ReturnAddress","TotalPrice",
            "AdditionalFee","AdditionalFeeDescription","TypeOrder","RejectedReason"
                ,"IsUserDelivery","PaymentMethod","DeliveryTime","DeliveryDate","ReturnDate","DurationDays","DurationMonths","Status","IsPaid","OrderBoxDetails","OrderHistoryExtensions"
                ,"OrderAdditionalFees"
        };
        [JsonProperty("id")]
        public Guid? Id { get; set; }
        [JsonProperty("customerName")]
        [BindNever]
        public string CustomerName { get; set; }
        [JsonProperty("Name")]
        [BindNever]
        public string Name { get; set; }
        [JsonProperty("customerPhone")]
        [BindNever]
        public string CustomerPhone { get; set; }
        [JsonProperty("deliveryAddress")]
        [BindNever]
        public string DeliveryAddress { get; set; }
        [JsonProperty("returnAddress")]
        [BindNever]
        public string ReturnAddress { get; set; }
        [JsonProperty("totalPrice")]
        [BindNever]
        public decimal? TotalPrice { get; set; }
        [JsonProperty("additionalFee")]
        [BindNever]
        public decimal? AdditionalFee { get; set; }
        [JsonProperty("additionalFeeDescription")]
        [BindNever]
        public string AdditionalFeeDescription { get; set; }
        [JsonProperty("rejectedReason")]
        [BindNever]
        public string RejectedReason { get; set; }
        [JsonProperty("type")]
        [BindNever]
        public int? Type { get; set; }
        [JsonProperty("isUserDelivery")]
        [BindNever]
        public bool? IsUserDelivery { get; set; }
        [JsonProperty("deliveryDate")]
        [BindNever]
        public DateTime? DeliveryDate { get; set; }
        [JsonProperty("deliveryTime")]
        [BindNever]
        public string DeliveryTime { get; set; }
        [JsonProperty("returnDate")]
        [BindNever]
        public DateTime? ReturnDate { get; set; }
        [JsonProperty("returnTime")]
        [BindNever]
        public string ReturnTime { get; set; }
        [JsonProperty("paymentMethod")]
        [BindNever]
        public int? PaymentMethod { get; set; }
        [JsonProperty("durationDays")]
        [BindNever]
        public int? DurationDays { get; set; }
        [JsonProperty("durationMonths")]
        [BindNever]
        public int? DurationMonths { get; set; }
        [JsonProperty("status")]
        [BindNever]
        public int? Status { get; set; }
        [JsonProperty("isPaid")]
        [BindNever]
        public bool? IsPaid { get; set; }
        [JsonProperty("storageId")]
        [BindNever]
        public Guid? StorageId { get; set; }
        [JsonProperty("storageName")]
        [BindNever]
        public string StorageName { get; set; }


        [JsonProperty("orderHistoryExtensions")]
        [BindNever]
        public virtual ICollection<OrderHistoryExtensionViewModel> OrderHistoryExtensions { get; set; }
        [JsonProperty("orderAdditionalFees")]
        [BindNever]
        public virtual ICollection<OrderAdditionalFeeViewModel> OrderAdditionalFees { get; set; }
    }
}
