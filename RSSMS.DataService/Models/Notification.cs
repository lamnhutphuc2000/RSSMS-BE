using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Notification
    {
        public Notification()
        {
            NotificationDetails = new HashSet<NotificationDetail>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public int? OrderId { get; set; }
        public int? RequestId { get; set; }
        public string Note { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Status { get; set; }
        public int? Type { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order Order { get; set; }
        public virtual Request Request { get; set; }
        public virtual ICollection<NotificationDetail> NotificationDetails { get; set; }
    }
}
