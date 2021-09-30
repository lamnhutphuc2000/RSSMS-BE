using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels
{
    public partial class UserViewModel
    {
        public static string[] Fields = {
            "Id","RoleName","StorageId","StorageName","Name","Address","Phone","Email","IsActive","Images"
        };
        [BindNever]
        public int? Id { get; set; }
        [BindNever]
        public string RoleName { get; set; }
        [BindNever]
        public int? StorageId { get; set; }
        [BindNever]
        public string StorageName { get; set; }
        [String]
        public string Name { get; set; }
        [BindNever]
        public string Address { get; set; }
        [BindNever]
        public string Phone { get; set; }
        [BindNever]
        public string Email { get; set; }
        [BindNever]
        public bool? IsActive { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
    }

}
