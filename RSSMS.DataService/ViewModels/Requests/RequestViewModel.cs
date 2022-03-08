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
            "Id","OrderId","UserId","Type","Status","Note","DeliveryStaffName","ReturnDate","StorageId","StorageName","ReturnTime","ReturnAddress","FromDate","ToDate","DeliveryStaffPhone","CustomerName","CustomerPhone","RequestDetails","Schedules"
        };
        [BindNever]
        public Guid Id { get; set; }
        [BindNever]
        public Guid? OrderId { get; set; }
        [BindNever]
        public Guid? UserId { get; set; }
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
        public Guid? StorageId { get; set; }
        [BindNever]
        public string StorageName { get; set; }
        [BindNever]
        public DateTime? DeliveryDate { get; set; }
        [BindNever]
        public string DeliveryTime { get; set; }
        [BindNever]
        public string DeliveryAddress { get; set; }
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
