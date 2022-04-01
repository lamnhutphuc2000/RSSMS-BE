using System;

namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaCreateViewModel
    {
        // có dùng
        public string Name { get; set; }
        public Guid? StorageId { get; set; }
        public int? Type { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public string Description { get; set; }
    }
}
