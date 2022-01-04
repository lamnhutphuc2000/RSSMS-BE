using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Users
{
    public class UserUpdateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
    }
}
