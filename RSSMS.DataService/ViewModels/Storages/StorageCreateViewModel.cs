using System;
using System.Collections.Generic;
using System.Text;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageCreateViewModel
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Address { get; set; }
        public int? Status { get; set; }

    }
}
