using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Request
    {
        public Request()
        {
            OrderHistoryExtensions = new HashSet<OrderHistoryExtension>();
            RequestDetails = new HashSet<RequestDetail>();
            Schedules = new HashSet<Schedule>();
        }

        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? UserId { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public string ReturnAddress { get; set; }
        public string Note { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual Order Order { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<OrderHistoryExtension> OrderHistoryExtensions { get; set; }
        public virtual ICollection<RequestDetail> RequestDetails { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
