using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageCreateViewModel
    {
        public string Name { get; set; }

        public string Size { get; set; }

        public string Address { get; set; }

        public int? Status { get; set; }

        public int? Type { get; set; }

        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<UserListStaffViewModel> ListStaff { get; set; }
    }
}
