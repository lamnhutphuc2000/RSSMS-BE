using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class TransferDetail
    {
        public Guid Id { get; set; }
        public Guid? OrderDetailId { get; set; }
        public Guid? TransferId { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
        public virtual Transfer Transfer { get; set; }
    }
}
