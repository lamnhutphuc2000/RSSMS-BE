using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderDetail
    {
        public OrderDetail()
        {
            Boxes = new HashSet<Box>();
            Images = new HashSet<Image>();
        }

        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ServiceId { get; set; }
        public int? Amount { get; set; }
        public decimal? TotalPrice { get; set; }

        public virtual Order Order { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
