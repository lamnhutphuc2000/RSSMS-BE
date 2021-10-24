using RSSMS.DataService.ViewModels.Boxes;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaDetailViewModel
    {
        public int Id { get; set; }
        public int? StorageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public List<BoxUsageViewModel> BoxUsage { get; set; }
    }

}
