using System;

namespace RSSMS.DataService.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int RequestId { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public bool IsOwn { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
