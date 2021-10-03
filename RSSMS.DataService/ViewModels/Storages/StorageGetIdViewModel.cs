using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public partial class StorageGetIdViewModel
    {
        public static string[] Fields = {
            "ManagerName","Name","Size","Usage","Address","Status","Images"
        };

        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public string Size { get; set; }
        [BindNever]
        public string Address { get; set; }
        [BindNever]
        public int? RemainingTime { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public int? OrderId { get; set; }

        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<UserListStaffViewModel> ListUser { get; set; }
    }
}
