using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.RequestDetail
{
    public class RequestDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? ServiceId { get; set; }
        public int? Amount { get; set; }
        public double? Price { get; set; }
        public string ServiceName { get; set; }
        public string ServiceImageUrl { get; set; }
        public int ServiceType { get; set; }
        public string Note { get; set; }
    }
}
