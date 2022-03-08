using System;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleDeliveryViewModel
    {
        public string ScheduleTime { get; set; }
        public Guid? OrderId { get; set; }
        public string DeliveryAddress { get; set; }
        public Guid? RequestId { get; set; }
    }
}
