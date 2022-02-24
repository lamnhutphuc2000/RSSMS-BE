using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class NotificationDetail
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? NotificationId { get; set; }
        public bool? IsOwn { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreateDate { get; set; }

        public virtual Notification Notification { get; set; }
        public virtual User User { get; set; }
    }
}
