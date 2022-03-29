using System;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestAssignStorageViewModel
    {
        public Guid RequestId { get; set; }
        public Guid StorageId { get; set; }
    }
}
