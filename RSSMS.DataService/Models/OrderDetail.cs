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
            OrderDetailServiceMaps = new HashSet<OrderDetailServiceMap>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? FloorId { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }

        public virtual Floor Floor { get; set; }
        public virtual Order Order { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<OrderDetailServiceMap> OrderDetailServiceMaps { get; set; }
    }
}
