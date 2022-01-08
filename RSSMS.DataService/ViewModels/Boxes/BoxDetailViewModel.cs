using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxDetailViewModel
    {
        public int Id { get; set; }
        public int? ShelfId { get; set; }
        public string SizeType { get; set; }
        public string ShelfName { get; set; }
        public string AreaName { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public int? ProductId { get; set; }
    }
}
