using System;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCancelViewModel
    {
        public Guid Id { get; set; }
        public string CancelReason { get; set; }
    }
}
