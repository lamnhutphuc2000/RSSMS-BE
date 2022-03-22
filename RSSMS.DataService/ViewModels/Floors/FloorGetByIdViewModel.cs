using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Floors
{
    public class FloorGetByIdViewModel
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public double Usage { get; set; }
        public double Used { get; set; }
        public double Available { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public virtual ICollection<OrderDetailInFloorViewModel> OrderDetails { get; set; }
    }
}
