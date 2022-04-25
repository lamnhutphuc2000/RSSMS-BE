using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class RequestTimeline
    {
        public Guid? Id { get; set; }
        public Guid? RequestId { get; set; }
        public DateTime? Datetime { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }

        public virtual Request Request { get; set; }
    }
}
