﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountsViewModel
    {
        public static string[] Fields = {
            "Id","RoleName","StorageName","Gender","Birthdate","Name","Address","Phone","Email","IsActive","ImageUrl","StaffManageStorages","SheduleDay","DeliveryTimes"
        };
        [BindNever]
        public Guid? Id { get; set; }
        [String]
        public string Name { get; set; }
        [BindNever]
        public string Email { get; set; }
        [BindNever]
        public int? Gender { get; set; }
        [BindNever]
        public DateTime? Birthdate { get; set; }
        [BindNever]
        public string Address { get; set; }
        [BindNever]
        public string Phone { get; set; }
        [String]
        public string RoleName { get; set; }
        [BindNever]
        public string ImageUrl { get; set; }
        [BindNever]
        public virtual ICollection<StaffAssignStorageViewModel> StaffAssignStorages { get; set; }
        public DateTime? SheduleDay { get; set; }
        public ICollection<string> DeliveryTimes { get; set; }
    }
}
