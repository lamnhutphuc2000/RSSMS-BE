using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderTimelines
{
    public class OrderTimelinesViewModel
    {
        public static string[] Fields = {
            "Id","OrderId","Date","Time","Description","CreatedDate","CreatedBy"};
        [BindNever]
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        [BindNever]
        public DateTime? Date { get; set; }
        [BindNever]
        public TimeSpan? Time { get; set; }
        [BindNever]
        public string Description { get; set; }
        [BindNever]
        public DateTime? CreatedDate { get; set; }
        [BindNever]
        public Guid? CreatedBy { get; set; }

    }
}
