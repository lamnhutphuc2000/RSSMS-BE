using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class User
    {
        public User()
        {
            Images = new HashSet<Image>();
            NotificationDetails = new HashSet<NotificationDetail>();
            OrderCustomers = new HashSet<Order>();
            OrderManagers = new HashSet<Order>();
            ProductCreatedByNavigations = new HashSet<Product>();
            ProductModifiedByNavigations = new HashSet<Product>();
            Requests = new HashSet<Request>();
            Schedules = new HashSet<Schedule>();
            StaffManageStorages = new HashSet<StaffManageStorage>();
        }

        public Guid Id { get; set; }
        public int? RoleId { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string FirebaseId { get; set; }
        public string DeviceTokenId { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<NotificationDetail> NotificationDetails { get; set; }
        public virtual ICollection<Order> OrderCustomers { get; set; }
        public virtual ICollection<Order> OrderManagers { get; set; }
        public virtual ICollection<Product> ProductCreatedByNavigations { get; set; }
        public virtual ICollection<Product> ProductModifiedByNavigations { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<StaffManageStorage> StaffManageStorages { get; set; }
    }
}
