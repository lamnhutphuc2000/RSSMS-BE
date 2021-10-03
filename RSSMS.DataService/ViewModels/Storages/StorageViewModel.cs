using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.Images;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public partial class StorageViewModel
    {
        public static string[] Fields = {
            "Id","ManagerId","ManagerName","Name","Size","Usage","Address","Status","Images"
        };
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public int? ManagerId { get; set; }
        [BindNever]
        public string ManagerName { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public string Size { get; set; }
        [BindNever]
        public int? Usage { get; set; }
        [BindNever]
        public string Address { get; set; }
        [BindNever]
        public int? Status { get; set; }

        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

    }
}
