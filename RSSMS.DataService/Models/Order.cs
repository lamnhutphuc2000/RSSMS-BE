using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Order
    {
        public Order()
        {
            Boxes = new HashSet<Box>();
            Images = new HashSet<Image>();
            OrderBoxDetails = new HashSet<OrderBoxDetail>();
            OrderDetails = new HashSet<OrderDetail>();
            OrderStorageDetails = new HashSet<OrderStorageDetail>();
            Requests = new HashSet<Request>();
            Schedules = new HashSet<Schedule>();
        }

        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? ManagerId { get; set; }
        public string DeliveryAddress { get; set; }
        public string AddressReturn { get; set; }
        public decimal? TotalPrice { get; set; }
        public string RejectedReason { get; set; }
        public int? TypeOrder { get; set; }
        public bool? IsPaid { get; set; }
        public int? PaymentMethod { get; set; }
        public bool? IsUserDelivery { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual User Customer { get; set; }
        public virtual User Manager { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<OrderBoxDetail> OrderBoxDetails { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderStorageDetail> OrderStorageDetails { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
