using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.RequestDetails;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestByIdViewModel
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? UserId { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public int? Durations { get; set; }
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
        public DateTime? CreateDate { get; set; }
        public int? OrderType { get; set; }
        public int? DurationDays { get; set; }
        public int? DurationMonths { get; set; }
        [BindNever]
        public virtual ICollection<RequestDetailCreateViewModel> RequestDetails { get; set; }
        [BindNever]
        public virtual ICollection<ScheduleViewModel> Schedules { get; set; }
    }
}
