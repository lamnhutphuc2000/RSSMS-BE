using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Products
{
    public class ProductOrderViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? Type { get; set; }
        public int? Amount { get; set; }

    }
}
