using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleViewModel
    {
        public int? OrderId { get; set; }

        public string Address { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public List<UserViewModel> Users { get; set; }
    }
}
