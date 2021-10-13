using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public partial class StorageGetIdViewModel
    {


        public string Name { get; set; }
        public string ManagerName { get; set; }
        public string Size { get; set; }
        public string Address { get; set; }
        public int? Usage { get; set; }
        public int? Type { get; set; }
        public int? OrderId { get; set; }
        public OrderStorageViewModel OrderInfo { get; set; }

        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<UserListStaffViewModel> ListUser { get; set; }
        public int? Status { get; set; }
        public virtual ICollection<ManagerManageStorageViewModel> StaffManageStorages { get; set; }
    }
}
