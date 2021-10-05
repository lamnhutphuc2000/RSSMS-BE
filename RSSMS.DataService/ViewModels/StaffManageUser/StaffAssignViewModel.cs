using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.StaffManageUser
{
    public class StaffAssignViewModel
    {
        public int StorageId { get; set; }
        public virtual ICollection<int> UserAssigned { get; set; }
        public virtual ICollection<int> UserUnAssigned { get; set; }
    }
}
