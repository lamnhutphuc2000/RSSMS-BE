using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class NotificationDetail
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? NotificationId { get; set; }
        public bool? IsOwn { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreateDate { get; set; }

        public virtual Notification Notification { get; set; }
        public virtual User User { get; set; }
    }
}
