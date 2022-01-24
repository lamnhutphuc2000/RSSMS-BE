using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.BoxOrderDetails
{
    public class BoxOrderDetailUpdateViewModel
    {
        public int OrderDetailId { get; set; }
        public int BoxId { get; set; }
        public int NewBoxId { get; set; }
    }
}
