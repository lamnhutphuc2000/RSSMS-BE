using System.Collections.Generic;
using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.ViewModels.Products
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int? Unit { get; set; }
        public string Description { get; set; }
        public int? Tooltip { get; set; }
        public int? Type { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
    }
}
