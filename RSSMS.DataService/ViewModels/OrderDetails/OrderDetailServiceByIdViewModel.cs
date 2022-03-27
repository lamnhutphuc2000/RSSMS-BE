using System;
using Newtonsoft.Json;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailServiceByIdViewModel
    {
        [JsonProperty("serviceId")]
        public Guid ServiceId { get; set; }
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
        [JsonProperty("serviceUrl")]
        public string ServiceUrl { get; set; }
        [JsonProperty("serviceType")]
        public int? ServiceType { get; set; }
        [JsonProperty("amount")]
        public int Amount { get; set; }
        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }
    }
}
