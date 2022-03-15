using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Shelf
    {
        public Shelf()
        {
            Floors = new HashSet<Floor>();
        }

        public Guid Id { get; set; }
        public Guid AreaId { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public bool IsActive { get; set; }

        public virtual Area Area { get; set; }
        public virtual ICollection<Floor> Floors { get; set; }
    }
}
