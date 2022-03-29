using System;

namespace RSSMS.DataService.ViewModels.StaffAssignStorage
{
    public class StaffAssignStorageCreateViewModel
    {
        public Guid UserId { get; set; }
        public Guid StorageId { get; set; }
        public string RoleName { get; set; }
    }
}
