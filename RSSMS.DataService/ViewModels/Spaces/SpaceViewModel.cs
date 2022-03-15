using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Attributes;
using RSSMS.DataService.ViewModels.Floors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RSSMS.DataService.ViewModels.Spaces
{
    public class SpaceViewModel
    {
        public static string[] Fields = {
            "Id","AreaId","Name","Type","Floors"
        };
        [BindNever]
        public Guid? Id { get; set; }
        [Guid]
        public Guid? AreaId { get; set; }
        [String]
        public string Name { get; set; }
        [BindNever]
        public int? Type { get; set; }

        [BindNever]
        public virtual ICollection<FloorInSpaceViewModel> Floors { get; set; }

    }
}
