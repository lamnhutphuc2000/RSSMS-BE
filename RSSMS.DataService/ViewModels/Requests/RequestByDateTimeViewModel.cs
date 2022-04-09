
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestByDateTimeViewModel
    {
        public DateTime DeliveryDate { get; set; }
        public string DeliveryTime { get; set;}
        public List<Request> Requests { get; set; }
    }
}
