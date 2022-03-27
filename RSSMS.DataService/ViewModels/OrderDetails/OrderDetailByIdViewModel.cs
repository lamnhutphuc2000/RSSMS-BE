using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailByIdViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("floorId")]
        public Guid? FloorId { get; set; }
        [JsonProperty("height")]
        public decimal? Height { get; set; }
        [JsonProperty("width")]
        public decimal? Width { get; set; }
        [JsonProperty("length")]
        public decimal? Length { get; set; }
        [JsonProperty("serviceType")]
        public int? ServiceType { get; set; }
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
        [JsonProperty("servicePrice")]
        public decimal? ServicePrice { get; set; }
        [JsonProperty("serviceImageUrl")]
        public string ServiceImageUrl { get; set; }
        [JsonProperty("images")]
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        [JsonProperty("orderDetailServices")]
        public virtual ICollection<OrderDetailServiceByIdViewModel> OrderDetailServices { get; set; }
    }
}
