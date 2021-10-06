using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.StaffManageUser
{
    public class StaffAssignViewModel
    {
        public int StorageId { get; set; }
        public string StorageName { get; set; }
        public virtual ICollection<UserAssignedViewModel> UserAssigned { get; set; }
        public virtual ICollection<UserAssignedViewModel> UserUnAssigned { get; set; }
    }
}
