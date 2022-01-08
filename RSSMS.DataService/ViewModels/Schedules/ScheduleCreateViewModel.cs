using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleCreateViewModel
    {
        public DateTime? SheduleDay { get; set; }
        public string DeliveryTime { get; set; }
        public IList<int> OrderIds { get; set; }
        public IList<int> UserIds { get; set; }
    }
}
