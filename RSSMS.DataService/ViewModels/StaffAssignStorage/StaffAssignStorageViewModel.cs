using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.StaffAssignStorage
{
    public class StaffAssignStorageViewModel
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StorageId { get; set; }
    }
}
