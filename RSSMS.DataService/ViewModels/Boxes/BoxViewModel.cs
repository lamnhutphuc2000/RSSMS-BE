﻿using System;

namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxViewModel
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Status { get; set; }
        public int? ShelfId { get; set; }
        public string SizeType { get; set; }
    }
}
