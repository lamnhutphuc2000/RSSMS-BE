using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public partial class OrderStorageViewModel
    {
        public static string[] Fields = {
            "Id","CustomerId","CustomerName","CustomerPhone","RemainingTime"};
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public int? RemainingTime { get; set; }


    }
}
