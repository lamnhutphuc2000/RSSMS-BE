﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Shelves
{
    public class ShelfUpdateViewModel
    {
        public int? Id { get; set; }
        public int? Type { get; set; }
        public string Note { get; set; }
        public int? BoxesInWidth { get; set; }
        public int? BoxesInHeight { get; set; }
    }
}