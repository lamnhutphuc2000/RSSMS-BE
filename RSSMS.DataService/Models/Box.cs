using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Box
    {
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

        public virtual Order Order { get; set; }
        public virtual Shelf Shelf { get; set; }
    }
}
