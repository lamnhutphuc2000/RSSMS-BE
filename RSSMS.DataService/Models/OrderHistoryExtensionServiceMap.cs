using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderHistoryExtensionServiceMap
    {
        public Guid Id { get; set; }
        public Guid? OrderHistoryExtensionId { get; set; }
        public Guid? Serviceid { get; set; }
        public int? Amount { get; set; }
        public double? Price { get; set; }

        public virtual OrderHistoryExtension OrderHistoryExtension { get; set; }
        public virtual Service Service { get; set; }
    }
}
