using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderDetailServiceMap
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? OrderDetailId { get; set; }
        public int? Amount { get; set; }
        public decimal? TotalPrice { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
        public virtual Service Service { get; set; }
    }
}
