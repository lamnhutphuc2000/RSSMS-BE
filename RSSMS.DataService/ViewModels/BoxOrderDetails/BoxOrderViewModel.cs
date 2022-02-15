using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.BoxOrderDetails
{
    public class BoxOrderViewModel
    {
        [JsonProperty("boxId")]
        public int BoxId { get; set; }
        [JsonProperty("areaId")]
        public int AreaId { get; set; }
        [JsonProperty("areaName")]
        public string AreaName { get; set; }
        [JsonProperty("shelfId")]
        public int ShelfId { get; set; }
        [JsonProperty("shelfName")]
        public string ShelfName { get; set; }
    }
}
