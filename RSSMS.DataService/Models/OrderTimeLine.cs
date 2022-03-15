using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderTimeline
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }

        public virtual Order Order { get; set; }
    }
}
