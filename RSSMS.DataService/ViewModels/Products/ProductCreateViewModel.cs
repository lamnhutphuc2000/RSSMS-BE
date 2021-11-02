using System;
using System.Collections.Generic;
using System.Text;
using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.ViewModels.Products
{
    public class ProductCreateViewModel
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public string Unit { get; set; }
        public string Tooltip { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

    }
}
