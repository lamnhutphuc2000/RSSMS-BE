using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.Boxes;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Shelves
{
    public class ShelfViewModel
    {
        public static string[] Fields = {
            "Id","AreaId","Name","Type","Status","BoxesInWidth","BoxesInHeight","Boxes"
        };
        [BindNever]
        public Guid? Id { get; set; }
        [BindNever]
        public Guid? AreaId { get; set; }
        [String]
        public string Name { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public int? BoxesInWidth { get; set; }
        [BindNever]
        public int? BoxesInHeight { get; set; }
        [BindNever]
        public virtual ICollection<BoxViewModel> Boxes { get; set; }
    }
}
