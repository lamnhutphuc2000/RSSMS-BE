using Newtonsoft.Json;
using RSSMS.DataService.ViewModels.BoxOrderDetails;
using RSSMS.DataService.ViewModels.Images;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailsViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("productId")]
        public int ProductId { get; set; }
        [JsonProperty("productName")]
        public string ProductName { get; set; }
        [JsonProperty("price")]
        public decimal? Price { get; set; }
        [JsonProperty("amount")]
        public int? Amount { get; set; }
        [JsonProperty("productType")]
        public int? ProductType { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("images")]
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

        [JsonProperty("boxDetails")]
        public virtual ICollection<BoxOrderViewModel> BoxDetails { get; set; }

    }
}
