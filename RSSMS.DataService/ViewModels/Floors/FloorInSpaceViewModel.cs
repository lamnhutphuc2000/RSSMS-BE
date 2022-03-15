using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Floors
{
    public class FloorInSpaceViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Usage { get; set; }
    }
}
