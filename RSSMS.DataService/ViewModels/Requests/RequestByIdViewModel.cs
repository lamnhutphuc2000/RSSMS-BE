using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.RequestDetails;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestByIdViewModel
    {
        public static string[] Fields = {
            "Id","OrderId","UserId","Type","Status","Note","RequestDetails"
        };
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public int? OrderId { get; set; }
        [BindNever]
        public int? UserId { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public string DeliveryStaffName { get; set; }
        [BindNever]
        public string DeliveryStaffPhone { get; set; }
        [BindNever]
        public string Note { get; set; }
        public string CancelBy { get; set; }
        public string CancelByPhone { get; set; }
        [BindNever]
        public virtual ICollection<RequestDetailCreateViewModel> RequestDetails { get; set; }
        [BindNever]
        public virtual ICollection<ScheduleViewModel> Schedules { get; set; }
    }
}
