using Microsoft.AspNetCore.Http;

namespace RSSMS.DataService.ViewModels.Images
{
    public class AvatarImageCreateViewModel
    {
        public IFormFile File { get; set; }
        public string Url { get; set; }
    }
}
