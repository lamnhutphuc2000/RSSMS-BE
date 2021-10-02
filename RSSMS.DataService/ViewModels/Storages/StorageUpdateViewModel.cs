﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageUpdateViewModel
    {
        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public int? Usage { get; set; }
        public int? Type { get; set; }
        public string Address { get; set; }
        public int? Status { get; set; }

    }
}