using System;

namespace RSSMS.DataService.ViewModels.Shelves
{
    public class ShelfCreateViewModel
    {
        public Guid AreaId { get; set; }
        public int? Type { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int BoxesInWidth { get; set; }
        public int BoxesInHeight { get; set; }
        public int BoxSize { get; set; }
        public Guid ServiceId { get; set; }
    }
}
