using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxUsageViewModel
    {
        public int SizeType { get; set; }
        public double? Usage { get; set; }

        public int TotalBox { get; set; }
        public int BoxRemaining { get; set; }
    }
}
