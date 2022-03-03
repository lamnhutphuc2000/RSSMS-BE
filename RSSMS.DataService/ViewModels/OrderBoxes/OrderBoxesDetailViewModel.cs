using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderBoxes
{
    public class OrderBoxesDetailViewModel
    {
        public Guid OrderId { get; set; }
        
        public IList<Guid> BoxesId { get; set; }
    }
}
