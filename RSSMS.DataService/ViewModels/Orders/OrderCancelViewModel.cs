using System;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderCancelViewModel
    {
        public Guid Id { get; set; }
        public string RejectedReason { get; set; }
    }
}
