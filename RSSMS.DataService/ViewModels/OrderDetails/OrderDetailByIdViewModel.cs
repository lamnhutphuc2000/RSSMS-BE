using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Imports;
using RSSMS.DataService.ViewModels.TransferDetails;
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
        [JsonProperty("status")]
        public int? Status { get; set; }
        [JsonProperty("serviceId")]
        public Guid? ServiceId { get; set; }
        [JsonProperty("serviceType")]
        public int? ServiceType { get; set; }
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
        [JsonProperty("storageName")]
        public string StorageName { get; set; }
        [JsonProperty("areaName")]
        public string AreaName { get; set; }
        [JsonProperty("spaceName")]
        public string SpaceName { get; set; }
        [JsonProperty("floorName")]
        public string FloorName { get; set; }
        [JsonProperty("servicePrice")]
        public decimal? ServicePrice { get; set; }
        [JsonProperty("serviceImageUrl")]
        public string ServiceImageUrl { get; set; }
        [JsonProperty("importNote")]
        public string ImportNote { get; set; }
        [JsonProperty("importCode")]
        public string ImportCode { get; set; }
        [JsonProperty("images")]
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        [JsonProperty("orderDetailServices")]
        public virtual ICollection<OrderDetailServiceByIdViewModel> OrderDetailServices { get; set; }
        [JsonProperty("import")]
        public virtual ImportViewModel Import { get; set; }
        [JsonProperty("transferDetails")]
        public virtual ICollection<TransferDetailViewModel> TransferDetails { get; set; }
    }
}
