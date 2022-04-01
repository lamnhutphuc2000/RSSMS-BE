

using System;

namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaUpdateViewModel
    {
        // có dùng
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public int? Type { get; set; }
        public string Description { get; set; }
    }
}
