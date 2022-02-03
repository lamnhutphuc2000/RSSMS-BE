using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.NotificationDetails
{
    public class NotificationDetailViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? NotificationId { get; set; }
        public bool? IsOwn { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
