using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Storage
    {
        public Storage()
        {
            Areas = new HashSet<Area>();
            Orders = new HashSet<Order>();
            Requests = new HashSet<Request>();
            StaffAssignStorages = new HashSet<StaffAssignStorage>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public bool IsActive { get; set; }
        public Guid? ModifiedBy { get; set; }
        public TimeSpan? Time { get; set; }

        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<StaffAssignStorage> StaffAssignStorages { get; set; }
    }
}
