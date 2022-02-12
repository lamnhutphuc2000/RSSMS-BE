using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.RequestDetails;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestByIdViewModel
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? UserId { get; set; }
        public decimal? TotalPrice { get; set; }
        public string ReturnAddress { get; set; }
        public string ReturnTime { get; set; }
        public DateTime? OldReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
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
