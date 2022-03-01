using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageCreateViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }

        public int? Status { get; set; }
        public ImageCreateViewModel Image { get; set; }

        public virtual ICollection<UserListStaffViewModel> ListStaff { get; set; }
    }
}
