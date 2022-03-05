using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Box
    {
        public Guid Id { get; set; }
        public Guid? OrderDetailId { get; set; }
        public Guid? ShelfId { get; set; }
        public Guid? ServiceId { get; set; }
        public int? Status { get; set; }
        public Guid? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
        public virtual Service Service { get; set; }
        public virtual Shelf Shelf { get; set; }
    }
}
