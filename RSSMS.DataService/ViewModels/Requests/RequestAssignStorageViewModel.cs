using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestAssignStorageViewModel
    {
        public Guid RequestId { get; set; }
        public Guid StorageId { get; set; }
    }
}
