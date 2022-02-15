using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.OrderHistoryExtension;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Orders
{
    public class OrderByIdViewModel
    {
        public int? Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string DeliveryAddress { get; set; }
        public string AddressReturn { get; set; }
        public decimal? TotalPrice { get; set; }
        public string RejectedReason { get; set; }
        public int? TypeOrder { get; set; }
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
        public int? StorageId { get; set; }
        public string StorageName { get; set; }
        public virtual ICollection<OrderDetailByIdViewModel> OrderDetails { get; set; }
        public virtual ICollection<OrderHistoryExtensionViewModel> OrderHistoryExtensions { get; set; }
    }
}
