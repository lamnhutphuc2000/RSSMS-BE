using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.Areas;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Storages
{
    public partial class StorageViewModel
    {
        public static string[] Fields = {
            "Id","Name","Type","Status","Address","Description","ImageUrl","Height","Width","Length","Usage","ManagerName","Areas","DeliveryFee","DeliveryDistance"
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
        public decimal? Height { get; set; }
        [BindNever]
        public decimal? Width { get; set; }
        [BindNever]
        public decimal? Length { get; set; }
        [BindNever]
        public int? Usage { get; set; }
        [BindNever]
        public string ManagerName { get; set; }
        [BindNever]
        public List<AreaDetailViewModel> Areas { get; set; }
        [BindNever]
        public decimal? DeliveryFee { get; set; }
        public string DeliveryDistance { get; set; }
    }
}
