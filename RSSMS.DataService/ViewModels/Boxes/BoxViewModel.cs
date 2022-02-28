using System;

namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxViewModel
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Status { get; set; }
        public Guid? ShelfId { get; set; }
        public string SizeType { get; set; }
    }
}
