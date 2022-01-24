using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderDetail
    {
        public OrderDetail()
        {
            Images = new HashSet<Image>();
        }

        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Amount { get; set; }
        public double? TotalPrice { get; set; }
        public string Note { get; set; }
        public int Id { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
