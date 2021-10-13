using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Shelf
    {
        public Shelf()
        {
            Boxes = new HashSet<Box>();
        }

        public int Id { get; set; }
        public int? AreaId { get; set; }
        public int? Type { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
        public int? BoxesInWidth { get; set; }
        public int? BoxesInHeight { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual Area Area { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
    }
}
