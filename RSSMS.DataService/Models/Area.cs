using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Area
    {
        public Area()
        {
            Spaces = new HashSet<Space>();
        }

        public Guid Id { get; set; }
        public Guid StorageId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public bool IsActive { get; set; }
        public Guid? ModifiedBy { get; set; }

        public virtual Storage Storage { get; set; }
        public virtual ICollection<Space> Spaces { get; set; }
    }
}
