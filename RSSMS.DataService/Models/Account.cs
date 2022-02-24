using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Account
    {
        public Account()
        {
            Notifications = new HashSet<Notification>();
            Orders = new HashSet<Order>();
            Requests = new HashSet<Request>();
            Schedules = new HashSet<Schedule>();
            Services = new HashSet<Service>();
            StaffAssignStorages = new HashSet<StaffAssignStorage>();
        }

        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool? IsActive { get; set; }
        public string Phone { get; set; }
        public string FirebaseId { get; set; }
        public string DeviceTokenId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<StaffAssignStorage> StaffAssignStorages { get; set; }
    }
}
