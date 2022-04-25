using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.RequestTimelinesViewModel
{
    public class RequestTimelinesViewModel
    {
        public static string[] Fields = {
            "OrderId","RequestId","Datetime","Name","Description"};
        public Guid? OrderId { get; set; }
        [BindNever]
        public Guid? RequestId { get; set; }
        [BindNever]
        public DateTime? Datetime { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public string Description { get; set; }
    }
}
