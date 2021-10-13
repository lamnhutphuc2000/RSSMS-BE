using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.Boxes;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Shelves
{
    public class ShelfViewModel
    {
        public static string[] Fields = {
            "Id","AreaId","Type","Note","BoxesInWidth","BoxesInHeight"
        };
        [BindNever]
        public int? Id { get; set; }
        public int? AreaId { get; set; }
        public int? Type { get; set; }
        [BindNever]
        public string Note { get; set; }
        [BindNever]
        public int? BoxesInWidth { get; set; }
        [BindNever]
        public int? BoxesInHeight { get; set; }

        public virtual ICollection<BoxViewModel> Boxes { get; set; }
    }
}
