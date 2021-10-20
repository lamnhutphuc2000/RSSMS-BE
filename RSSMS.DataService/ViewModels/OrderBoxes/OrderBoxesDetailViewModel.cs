using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderBoxes
{
    public class OrderBoxesDetailViewModel
    {
        public int OrderId { get; set; }
        public IList<int> BoxesId { get; set; }
    }
}
