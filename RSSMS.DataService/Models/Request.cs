using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Request
    {
        public Request()
        {
            Notifications = new HashSet<Notification>();
            OrderTimelines = new HashSet<OrderTimeline>();
            RequestDetails = new HashSet<RequestDetail>();
            RequestTimelines = new HashSet<RequestTimeline>();
            Schedules = new HashSet<Schedule>();
        }

        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? StorageId { get; set; }
        public int Type { get; set; }
        public int? Status { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string Note { get; set; }
        public int? TypeOrder { get; set; }
        public bool IsActive { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsCustomerDelivery { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public string ReturnAddress { get; set; }
        public DateTime? ReturnDate { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? OldReturnDate { get; set; }
        public decimal? DepositFee { get; set; }
        public decimal? AdvanceMoney { get; set; }

        public virtual Account CreatedByNavigation { get; set; }
        public virtual Order Order { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<OrderTimeline> OrderTimelines { get; set; }
        public virtual ICollection<RequestDetail> RequestDetails { get; set; }
        public virtual ICollection<RequestTimeline> RequestTimelines { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
