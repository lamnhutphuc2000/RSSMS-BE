﻿using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Storages
{
    public class StorageDetailViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? Usage { get; set; }
        public OrderAreaViewModel OrderInfo { get; set; }
        public virtual ICollection<UserListStaffViewModel> ListUser { get; set; }
        public virtual ICollection<ManagerManageStorageViewModel> StaffManageStorages { get; set; }
    }
}