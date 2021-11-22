using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderAreas
{
    public class OrderAreaDetailViewModel
    {
        public int OrderId { get; set; }
        public List<int> AreaIds { get; set; }
    }
}
