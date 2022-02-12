using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.BoxOrderDetails
{
    public class BoxOrderDetailCreateViewModel
    {
        public int OrderId { get; set; }
        public ICollection<BoxOrderDetailViewModel> Boxes { get; set; }
    }
}
