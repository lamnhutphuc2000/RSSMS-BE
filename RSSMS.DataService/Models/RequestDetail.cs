using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class RequestDetail
    {
        public Guid Id { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? ServiceId { get; set; }
        public int? Amount { get; set; }
        public decimal? Price { get; set; }
        public string Note { get; set; }

        public virtual Request Request { get; set; }
        public virtual Service Service { get; set; }
    }
}
