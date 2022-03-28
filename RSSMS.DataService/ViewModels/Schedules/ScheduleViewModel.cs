using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Requests;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Schedules
{
    public class ScheduleViewModel
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? RequestId { get; set; }
        public DateTime ScheduleDay { get; set; }
        public string ScheduleTime { get; set; }
        public int? RequestType { get; set; }
        public virtual List<RequestViewModel> Requests { get; set; }
        public virtual RequestViewModel Request { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }

        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public List<AccountsViewModel> Accounts { get; set; }
    }
}
