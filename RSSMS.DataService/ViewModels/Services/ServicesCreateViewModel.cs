﻿using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Services
{
    public class ServicesCreateViewModel
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public string Unit { get; set; }
        public string Tooltip { get; set; }
        public ImageCreateViewModel Image { get; set; }
    }
}