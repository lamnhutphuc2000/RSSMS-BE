using System;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestUpdateViewModel
    {
        public Guid Id { get; set; }
        public bool IsPaid { get; set; }
    }
}
