using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.RequestDetails;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestViewModel
    {
        public static string[] Fields = {
            "Id","OrderId","UserId","Type","Status","Note","DeliveryStaffName","ReturnDate","ReturnTime","ReturnAddress","FromDate","ToDate","DeliveryStaffPhone","CustomerName","CustomerPhone","RequestDetails","Schedules"
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
        public string CustomerName { get; set; }
        [BindNever]
        public string CustomerPhone { get; set; }
        [BindNever]
        public DateTime? ReturnDate { get; set; }
        [BindNever]
        public string ReturnTime { get; set; }
        [BindNever]
        public string ReturnAddress { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [BindNever]
        public string Note { get; set; }
        [BindNever]
        public virtual ICollection<RequestDetailCreateViewModel> RequestDetails { get; set; }
        [BindNever]
        public virtual ICollection<ScheduleViewModel> Schedules { get; set; }
    }
}
