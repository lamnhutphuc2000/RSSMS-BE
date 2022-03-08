using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime? ScheduleDay { get; set; }
        public string DeliveryTime { get; set; }
        public IList<UserViewModel> Users { get; set; }
    }
}
