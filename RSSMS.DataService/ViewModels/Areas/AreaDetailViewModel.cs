using RSSMS.DataService.ViewModels.Boxes;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid? StorageId { get; set; }
        public string Name { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public int? Type { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public List<BoxUsageViewModel> BoxUsage { get; set; }
    }

}
