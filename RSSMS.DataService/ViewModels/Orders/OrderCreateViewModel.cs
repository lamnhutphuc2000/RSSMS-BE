using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderCreateViewModel
    {
        public Guid? CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReturnAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string RejectedReason { get; set; }
        public int? Duration { get; set; }
        public int? Type { get; set; }
        public bool IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public virtual ICollection<OrderDetailViewModel> OrderDetails { get; set; }
    }
}
