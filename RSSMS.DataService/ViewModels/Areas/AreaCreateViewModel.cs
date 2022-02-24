﻿using System;

namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaCreateViewModel
    {
        public string Name { get; set; }
        public Guid? StorageId { get; set; }
        public int? Type { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
    }
}
