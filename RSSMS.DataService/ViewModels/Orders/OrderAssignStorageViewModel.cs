using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderAssignStorageViewModel
    {
        public Guid OrderId { get; set; }
        public Guid StorageId { get; set; }
    }
}
