using RSSMS.DataService.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.StaffAssignStorage
{
    public class StaffAssignInStorageViewModel
    {
        public Guid StorageId { get; set; }
        public string StorageName { get; set; }
        public virtual ICollection<AccountsAssignedViewModel> UserAssigned { get; set; }
        public virtual ICollection<AccountsAssignedViewModel> UserUnAssigned { get; set; }
    }
}
