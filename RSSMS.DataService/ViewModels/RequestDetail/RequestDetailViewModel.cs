using System;
using Newtonsoft.Json;

namespace RSSMS.DataService.ViewModels.RequestDetail
{
    public class RequestDetailViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("requestId")]
        public Guid? RequestId { get; set; }
        [JsonProperty("serviceId")]
        public Guid? ServiceId { get; set; }
        [JsonProperty("amount")]
        public int? Amount { get; set; }
        [JsonProperty("price")]
        public double? Price { get; set; }
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
        [JsonProperty("serviceImageUrl")]
        public string ServiceImageUrl { get; set; }
        [JsonProperty("serviceType")]
        public int ServiceType { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
    }
}
