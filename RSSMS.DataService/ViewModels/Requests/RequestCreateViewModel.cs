using System;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCreateViewModel
    {
        public DateTime CancelDay { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
    }
}
