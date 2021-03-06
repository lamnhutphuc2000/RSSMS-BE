using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderHistoryExtension
    {
        public OrderHistoryExtension()
        {
            OrderHistoryExtensionServiceMaps = new HashSet<OrderHistoryExtensionServiceMap>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? RequestId { get; set; }
        public DateTime OldReturnDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Note { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime? PaidDate { get; set; }
        public Guid? ModifiedBy { get; set; }

        public virtual Order Order { get; set; }
        public virtual ICollection<OrderHistoryExtensionServiceMap> OrderHistoryExtensionServiceMaps { get; set; }
    }
}
