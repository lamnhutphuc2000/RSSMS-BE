using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.ViewModels.Products
{
    public class ProductViewAllModel
    {
        public static string[] Fields = {
            "Id","Name","Price","Name","Size","Description","Type","Status","Unit","Tooltip"
        };
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public decimal? Price { get; set; }
        [BindNever]
        public int? Size { get; set; }
        [BindNever]
        public string Description { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public string Unit { get; set; }
        [BindNever]
        public string Tooltip { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

    }
}
