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
            Schedules = new HashSet<Schedule>();
        }

        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string Note { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }

        public virtual Account CreatedByNavigation { get; set; }
        public virtual Order Order { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
