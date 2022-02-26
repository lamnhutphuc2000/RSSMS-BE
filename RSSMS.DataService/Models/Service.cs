using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Service
    {
        public Service()
        {
            Boxes = new HashSet<Box>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Note { get; set; }
        public decimal? Price { get; set; }
        public int? Type { get; set; }
        public string Size { get; set; }
        public string ToolTip { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public bool? IsActive { get; set; }

        public virtual Account Account { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
