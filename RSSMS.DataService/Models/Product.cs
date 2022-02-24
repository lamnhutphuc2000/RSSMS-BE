using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Product
    {
        public Product()
        {
            Boxes = new HashSet<Box>();
            Images = new HashSet<Image>();
            OrderDetails = new HashSet<OrderDetail>();
            Storages = new HashSet<Storage>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public string ToolTip { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string Size { get; set; }

        public virtual User CreatedByNavigation { get; set; }
        public virtual User ModifiedByNavigation { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Storage> Storages { get; set; }
    }
}
