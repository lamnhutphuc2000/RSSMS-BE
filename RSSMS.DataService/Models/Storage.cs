﻿using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Storage
    {
        public Storage()
        {
            Areas = new HashSet<Area>();
            StaffAssignStorages = new HashSet<StaffAssignStorage>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }

        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<StaffAssignStorage> StaffAssignStorages { get; set; }
    }
}
