using System;

namespace RSSMS.DataService.ViewModels.Spaces
{
    public class SpaceUpdateViewModel
    {
        public Guid Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int NumberOfFloor { get; set; }
        public decimal FloorWidth { get; set; }
        public decimal FloorLength { get; set; }
        public decimal FloorHeight { get; set; }
    }
}
