using Newtonsoft.Json;
using System;

namespace RSSMS.DataService.ViewModels.Images
{
    public partial class AvatarImageViewModel
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("file")]
        public string File { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
