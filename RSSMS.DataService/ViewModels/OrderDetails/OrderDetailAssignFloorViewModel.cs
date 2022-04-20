using System;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailAssignFloorViewModel
    {
        public Guid OrderDetailId { get; set; }
        public Guid FloorId { get; set; }
        public int ServiceType { get; set; }
        public string ImportNote { get; set; }
    }
}
