using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleViewModel
    {
        public int? OrderId { get; set; }
        public DateTime ScheduleDay { get; set; }
        public OrderViewModel Order { get; set; }
        public List<OrderViewModel> Orders { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }

        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public List<UserViewModel> Users { get; set; }
    }
}
