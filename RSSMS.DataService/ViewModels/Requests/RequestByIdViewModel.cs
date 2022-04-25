using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.RequestDetail;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestByIdViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("orderId")]
        public Guid? OrderId { get; set; }
        [JsonProperty("userId")]
        public Guid? UserId { get; set; }
        [JsonProperty("customerId")]
        public Guid? CustomerId { get; set; }
        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
        [JsonProperty("customerPhone")]
        public string CustomerPhone { get; set; }
        [JsonProperty("storageName")]
        public string StorageName { get; set; }
        [JsonProperty("storageAddress")]
        public string StorageAddress { get; set; }
        [JsonProperty("totalPrice")]
        public decimal? TotalPrice { get; set; }
        [JsonProperty("isPaid")]
        public bool? IsPaid { get; set; }
        [JsonProperty("deliveryDate")]
        public DateTime? DeliveryDate { get; set; }
        [JsonProperty("deliveryTime")]
        public string DeliveryTime { get; set; }
        [JsonProperty("advanceMoney")]
        public decimal? AdvanceMoney { get; set; }
        [JsonProperty("deliveryAddress")]
        public string DeliveryAddress { get; set; }
        [JsonProperty("isCustomerDelivery")]
        public bool? IsCustomerDelivery { get; set; }
        [JsonProperty("returnAddress")]
        public string ReturnAddress { get; set; }
        [JsonProperty("returnDate")]
        public DateTime? ReturnDate { get; set; }
        [JsonProperty("returnTime")]
        public string ReturnTime { get; set; }
        [JsonProperty("cancelReason")]
        public string CancelReason { get; set; }
        [JsonProperty("durations")]
        public int? Durations { get; set; }
        [JsonProperty("type")]
        public int? Type { get; set; }
        [JsonProperty("typeOrder")]
        public int? TypeOrder { get; set; }
        [JsonProperty("status")]
        public int? Status { get; set; }
        [JsonProperty("deliveryStaffName")]
        public string DeliveryStaffName { get; set; }
        [JsonProperty("deliveryStaffPhone")]
        public string DeliveryStaffPhone { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("deliveryFee")]
        public decimal? DeliveryFee { get; set; }
        [JsonProperty("cancelBy")]
        public string CancelBy { get; set; }
        [JsonProperty("cancelByPhone")]
        public string CancelByPhone { get; set; }
        [JsonProperty("createDate")]
        public DateTime? CreateDate { get; set; }
        [JsonProperty("cancelDate")]
        public DateTime? CancelDate { get; set; }
        [JsonProperty("orderType")]
        public int? OrderType { get; set; }
        [JsonProperty("durationDays")]
        public int? DurationDays { get; set; }
        [JsonProperty("durationMonths")]
        public int? DurationMonths { get; set; }
        [JsonProperty("oldReturnDate")]
        public DateTime? OldReturnDate { get; set; }
        [JsonProperty("dequestDetails")]
        public virtual ICollection<RequestDetailViewModel> RequestDetails { get; set; }
    }
}
