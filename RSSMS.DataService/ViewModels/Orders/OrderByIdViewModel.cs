using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.OrderAdditionalFees;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.OrderHistoryExtension;
using RSSMS.DataService.ViewModels.Requests;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderByIdViewModel
    {
        [JsonProperty("id")]
        public Guid? Id { get; set; }
        [JsonProperty("customerId")]
        public Guid? CustomerId { get; set; }
        [JsonProperty("requestId")]
        public Guid? RequestId { get; set; }
        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
        [JsonProperty("customerPhone")]
        public string CustomerPhone { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("deliveryAddress")]
        public string DeliveryAddress { get; set; }
        [JsonProperty("returnAddress")]
        public string ReturnAddress { get; set; }
        [JsonProperty("totalPrice")]
        public decimal? TotalPrice { get; set; }
        [JsonProperty("additionalFee")]
        public double? AdditionalFee { get; set; }
        [JsonProperty("additionalFeeDescription")]
        public string AdditionalFeeDescription { get; set; }
        [JsonProperty("compensationFee")]
        public decimal? CompensationFee { get; set; }
        [JsonProperty("compensationDescription")]
        public string CompensationDescription { get; set; }
        [JsonProperty("rejectedReason")]
        public string RejectedReason { get; set; }
        [JsonProperty("type")]
        public int? Type { get; set; }
        [JsonProperty("isUserDelivery")]
        public bool? IsUserDelivery { get; set; }
        [JsonProperty("deliveryDate")]
        public DateTime? DeliveryDate { get; set; }
        [JsonProperty("deliveryTime")]
        public string DeliveryTime { get; set; }
        [JsonProperty("returnDate")]
        public DateTime? ReturnDate { get; set; }
        [JsonProperty("returnTime")]
        public string ReturnTime { get; set; }
        [JsonProperty("paymentMethod")]
        public int? PaymentMethod { get; set; }
        [JsonProperty("durationDays")]
        public int? DurationDays { get; set; }
        [JsonProperty("durationMonths")]
        public int? DurationMonths { get; set; }
        [JsonProperty("status")]
        public int? Status { get; set; }
        [JsonProperty("isPaid")]
        public bool? IsPaid { get; set; }
        [JsonProperty("storageId")]
        public Guid? StorageId { get; set; }
        [JsonProperty("storageAddress")]
        public string StorageAddress { get; set; }
        [JsonProperty("storageName")]
        public string StorageName { get; set; }
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("createdBy")]
        public Guid? CreatedBy { get; set; }
        [JsonProperty("importDeliveryBy")]
        public string ImportDeliveryBy { get; set; }
        [JsonProperty("importDay")]
        public DateTime? ImportDay { get; set; }
        [JsonProperty("importStaff")]
        public string ImportStaff { get; set; }
        [JsonProperty("importCode")]
        public string ImportCode { get; set; }
        [JsonProperty("requests")]
        public virtual ICollection<RequestByIdViewModel> Requests { get; set; }
        [JsonProperty("orderDetails")]
        public virtual ICollection<OrderDetailByIdViewModel> OrderDetails { get; set; }
        [JsonProperty("orderHistoryExtensions")]
        public virtual ICollection<OrderHistoryExtensionViewModel> OrderHistoryExtensions { get; set; }
        [JsonProperty("orderAdditionalFees")]
        public virtual ICollection<OrderAdditionalFeeViewModel> OrderAdditionalFees { get; set; }
    }
}
