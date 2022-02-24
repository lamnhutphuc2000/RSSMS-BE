#nullable disable

using System;

namespace RSSMS.DataService.Models
{
    public partial class OrderStorageDetail
    {
        public Guid OrderId { get; set; }
        public Guid StorageId { get; set; }
        public Guid Id { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order Order { get; set; }
        public virtual Storage Storage { get; set; }
    }
}
