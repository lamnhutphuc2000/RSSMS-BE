using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleCreateViewModel
    {
        public int OrderId { get; set; }
        public DateTime? SheduleDay { get; set; }
        public string DeliveryTime { get; set; }
        public IList<int> UserIds { get; set; }
    }
}
