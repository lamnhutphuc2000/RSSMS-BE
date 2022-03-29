using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using System;

namespace RSSMS.DataService.ViewModels.OrderTimelines
{
    public class OrderTimelinesViewModel
    {
        public static string[] Fields = {
            "Id","OrderId","RequestId","Datetime","Name","Description"};
        [BindNever]
        public Guid? Id { get; set; }
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
