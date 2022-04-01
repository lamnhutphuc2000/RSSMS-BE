using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountCreateViewModel
    {
        // có dùng
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public Guid? RoleId { get; set; }
        public string DeviceToken { get; set; }
        public AvatarImageCreateViewModel Image { get; set; }
        public virtual ICollection<Guid> StorageIds { get; set; }
    }
}
