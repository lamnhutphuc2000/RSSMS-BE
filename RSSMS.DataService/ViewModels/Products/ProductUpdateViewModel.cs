using RSSMS.DataService.ViewModels.Images;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Products
{
    public class ProductUpdateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public string Unit { get; set; }
        public string Tooltip { get; set; }
        public int? Status { get; set; }
        public virtual List<AvatarImageViewModel> Images { get; set; }
    }
}
