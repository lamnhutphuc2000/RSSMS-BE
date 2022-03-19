using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCancelViewModel
    {
        public Guid Id { get; set; }
        public string CancelReason { get; set; }
    }
}
