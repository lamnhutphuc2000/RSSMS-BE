using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public partial class StorageViewModel
    {
        public static string[] Fields = {
            "Id","Name","Type","Status","Address","Description","ImageUrl","Usage"
        };
        [BindNever]
        public Guid? Id { get; set; }
        [String]
        public string Name { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public string Address { get; set; }
        [BindNever]
        public string Description { get; set; }
        [BindNever]
        public string ImageUrl { get; set; }
        [BindNever]
        public int? Usage { get; set; }


    }
}
