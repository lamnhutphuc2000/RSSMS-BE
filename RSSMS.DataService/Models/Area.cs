using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Area
    {
        public Area()
        {
            Shelves = new HashSet<Shelf>();
        }

        public int Id { get; set; }
        public int? StorageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Usage { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual Storage Storage { get; set; }
        public virtual ICollection<Shelf> Shelves { get; set; }
    }
}
