using System;

namespace RSSMS.DataService.ViewModels.Shelves
{
    public class ShelfUpdateViewModel
    {
        public Guid Id { get; set; }
        public int? Type { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public int BoxesInWidth { get; set; }
        public int BoxesInHeight { get; set; }
        public int BoxSize { get; set; }
        public Guid ServiceId { get; set; }
    }
}
