using Newtonsoft.Json;

namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxDetailViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("shelfId")]
        public int? ShelfId { get; set; }
        [JsonProperty("sizeType")]
        public string SizeType { get; set; }
        [JsonProperty("shelfName")]
        public string ShelfName { get; set; }
        [JsonProperty("areaName")]
        public string AreaName { get; set; }
        [JsonProperty("status")]
        public int? Status { get; set; }
        [JsonProperty("isActive")]
        public bool? IsActive { get; set; }
        [JsonProperty("productId")]
        public int? ProductId { get; set; }
    }
}
