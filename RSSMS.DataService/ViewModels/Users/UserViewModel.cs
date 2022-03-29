//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using RSSMS.DataService.Attributes;
//using RSSMS.DataService.ViewModels.Images;
//using RSSMS.DataService.ViewModels.StaffManageUser;
//using System;
//using System.Collections.Generic;

//namespace RSSMS.DataService.ViewModels.Users
//{
//    public partial class UserViewModel
//    {
//        public static string[] Fields = {
//            "Id","RoleName","StorageName","Gender","Birthdate","Name","Address","Phone","Email","IsActive","Images","StaffManageStorages"
//        };
//        [BindNever]
//        public int? Id { get; set; }
//        [String]
//        public string Name { get; set; }
//        [BindNever]
//        public string Email { get; set; }
//        [BindNever]
//        public int? Gender { get; set; }
//        [BindNever]
//        public DateTime? Birthdate { get; set; }
//        [BindNever]
//        public string Address { get; set; }
//        [BindNever]
//        public string Phone { get; set; }
//        [String]
//        public string RoleName { get; set; }
//        [BindNever]
//        public bool? IsActive { get; set; }
//        [BindNever]
//        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
//        [BindNever]
//        public virtual ICollection<StaffManageStorageViewModel> StaffManageStorages { get; set; }
//        public DateTime? SheduleDay { get; set; }
//        public ICollection<string> DeliveryTimes { get; set; }
//    }
//}
