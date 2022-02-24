using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageUpdateViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public int? Usage { get; set; }

        public string Address { get; set; }
        public int? Status { get; set; }
        public AvatarImageCreateViewModel Image { get; set; }

    }
}
