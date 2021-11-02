using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderStorages
{
    public class OrderStorageDetailViewModel
    {
        public int OrderId { get; set; }
        public List<int> StorageIds { get; set; }
    }
}
