using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleCreateViewModel
    {
        public DateTime? SheduleDay { get; set; }
        public IList<ScheduleDeliveryViewModel> Schedules { get; set; }
        public IList<Guid> UserIds { get; set; }
    }
}
