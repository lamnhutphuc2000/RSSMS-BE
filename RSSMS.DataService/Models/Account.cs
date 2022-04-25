using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Account
    {
        public Account()
        {
            ExportCreatedByNavigations = new HashSet<Export>();
            ExportDeliveryByNavigations = new HashSet<Export>();
            ImportCreatedByNavigations = new HashSet<Import>();
            ImportDeliveryByNavigations = new HashSet<Import>();
            Notifications = new HashSet<Notification>();
            Orders = new HashSet<Order>();
            Requests = new HashSet<Request>();
            Schedules = new HashSet<Schedule>();
            StaffAssignStorages = new HashSet<StaffAssignStorage>();
            Transfers = new HashSet<Transfer>();
        }

        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string FirebaseId { get; set; }
        public string DeviceTokenId { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Export> ExportCreatedByNavigations { get; set; }
        public virtual ICollection<Export> ExportDeliveryByNavigations { get; set; }
        public virtual ICollection<Import> ImportCreatedByNavigations { get; set; }
        public virtual ICollection<Import> ImportDeliveryByNavigations { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<StaffAssignStorage> StaffAssignStorages { get; set; }
        public virtual ICollection<Transfer> Transfers { get; set; }
    }
}
