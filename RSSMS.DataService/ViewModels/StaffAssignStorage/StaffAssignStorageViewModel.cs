using System;

namespace RSSMS.DataService.ViewModels.StaffAssignStorage
{
    public class StaffAssignStorageViewModel
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StorageId { get; set; }
    }
}
