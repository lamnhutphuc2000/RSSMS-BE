using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.OrderHistoryExtension;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderByIdViewModel
    {
        public Guid? Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Name { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReturnAddress { get; set; }
        public decimal? TotalPrice { get; set; }
        public string RejectedReason { get; set; }
        public int? Type { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public int? PaymentMethod { get; set; }
        public int? DurationDays { get; set; }
        public int? DurationMonths { get; set; }
        public int? Status { get; set; }
        public bool? IsPaid { get; set; }
        public Guid? StorageId { get; set; }
        public string StorageName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public virtual ICollection<OrderDetailByIdViewModel> OrderDetails { get; set; }
        public virtual ICollection<OrderHistoryExtensionViewModel> OrderHistoryExtensions { get; set; }
    }
}
