using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Service
    {
        public Service()
        {
            OrderDetailServiceMaps = new HashSet<OrderDetailServiceMap>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Note { get; set; }
        public decimal Price { get; set; }
        public int? Type { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public string ToolTip { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<OrderDetailServiceMap> OrderDetailServiceMaps { get; set; }
    }
}
