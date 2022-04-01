using System;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime? ScheduleDay { get; set; }
        public string DeliveryTime { get; set; }
    }
}
