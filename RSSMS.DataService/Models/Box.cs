using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Box
    {
        public Box()
        {
            BoxOrderDetails = new HashSet<BoxOrderDetail>();
            RequestDetails = new HashSet<RequestDetail>();
        }

        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? ShelfId { get; set; }
        public int? SizeType { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ProductId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Shelf Shelf { get; set; }
        public virtual ICollection<BoxOrderDetail> BoxOrderDetails { get; set; }
        public virtual ICollection<RequestDetail> RequestDetails { get; set; }
    }
}
