using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.BoxOrderDetails
{
    public class BoxOrderDetailCreateViewModel
    {
        public int OrderId { get; set; }
        public ICollection<BoxOrderDetailViewModel> Boxes { get; set; }
    }
}
