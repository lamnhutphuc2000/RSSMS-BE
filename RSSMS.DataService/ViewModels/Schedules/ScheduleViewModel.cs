using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.Requests;
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
        private List<RequestScheduleViewModel> _Requests { get; set; }
        public virtual List<RequestScheduleViewModel> Requests
        {
            
            get { _Requests.Sort(); return _Requests; }
            set { _Requests = value; }
        }
        public virtual RequestScheduleViewModel Request { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }

        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public List<AccountViewModel> Accounts { get; set; }
    }
}
