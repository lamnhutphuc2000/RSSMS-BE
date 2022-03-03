using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            OrderHistoryExtensions = new HashSet<OrderHistoryExtension>();
            OrderTimelines = new HashSet<OrderTimeline>();
            Requests = new HashSet<Request>();
        }

        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? StorageId { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReturnAddress { get; set; }
        public decimal? TotalPrice { get; set; }
        public string RejectedReason { get; set; }
        public int? Type { get; set; }
        public bool? IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual Account Customer { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderHistoryExtension> OrderHistoryExtensions { get; set; }
        public virtual ICollection<OrderTimeline> OrderTimelines { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
    }
}
