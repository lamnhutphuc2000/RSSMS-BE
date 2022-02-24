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

        public Guid Id { get; set; }
        public Guid? AreaId { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public int? BoxesInWidth { get; set; }
        public int? BoxesInHeight { get; set; }
        public bool? IsActive { get; set; }

        public virtual Area Area { get; set; }
        public virtual ICollection<Box> Boxes { get; set; }
    }
}
