using System;
using Newtonsoft.Json;

namespace RSSMS.DataService.ViewModels.OrderHistoryExtension
{
    public class OrderHistoryExtensionViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("orderId")]
        public Guid? OrderId { get; set; }
        [JsonProperty("requestId")]
        public Guid? RequestId { get; set; }
        [JsonProperty("oldReturnDate")]
        public DateTime? OldReturnDate { get; set; }
        [JsonProperty("returnDate")]
        public DateTime? ReturnDate { get; set; }
        [JsonProperty("status")]
        public int? Status { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("totalPrice")]
        public decimal? TotalPrice { get; set; }
        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }
        [JsonProperty("createDate")]
        public DateTime? CreateDate { get; set; }
        [JsonProperty("modifiedBy")]
        public Guid? ModifiedBy { get; set; }
        [JsonProperty("paidDate")]
        public DateTime? PaidDate { get; set; }
    }
}
