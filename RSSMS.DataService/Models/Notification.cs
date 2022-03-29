using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Notification
    {
        public Guid Id { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? RequestId { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Account Receiver { get; set; }
        public virtual Request Request { get; set; }
    }
}
