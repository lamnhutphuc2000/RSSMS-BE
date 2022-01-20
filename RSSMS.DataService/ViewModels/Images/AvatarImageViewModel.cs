using Newtonsoft.Json;

namespace RSSMS.DataService.ViewModels.Images
{
    public partial class AvatarImageViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
