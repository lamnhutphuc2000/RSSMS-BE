using System;

namespace RSSMS.DataService.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }
        public Guid? RequestId { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid ReceiverId { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
