using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Export
    {
        public Export()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public Guid Id { get; set; }
        public Guid? FloorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Code { get; set; }
        public Guid? DeliveryBy { get; set; }
        public string ReturnAddress { get; set; }

        public virtual Account CreatedByNavigation { get; set; }
        public virtual Floor Floor { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
