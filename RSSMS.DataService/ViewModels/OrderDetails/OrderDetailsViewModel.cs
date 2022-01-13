using RSSMS.DataService.ViewModels.Images;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailsViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? Amount { get; set; }
        public int? ProductType { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

    }
}
