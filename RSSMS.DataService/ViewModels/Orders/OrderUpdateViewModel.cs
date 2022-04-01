using System;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderUpdateViewModel
    {
        public Guid Id { get; set; }
        public bool? IsUserDelivery { get; set; }
        public bool? IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string RejectedReason { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReturnAddress { get; set; }
        public int? Status { get; set; }
    }
}
