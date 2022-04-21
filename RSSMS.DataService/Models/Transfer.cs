using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Transfer
    {
        public Transfer()
        {
            TransferDetails = new HashSet<TransferDetail>();
        }

        public Guid Id { get; set; }
        public Guid? FloorFromId { get; set; }
        public Guid? FloorToId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }

        public virtual Floor FloorFrom { get; set; }
        public virtual Floor FloorTo { get; set; }
        public virtual ICollection<TransferDetail> TransferDetails { get; set; }
    }
}
