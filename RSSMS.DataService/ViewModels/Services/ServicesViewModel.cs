using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Services
{
    public class ServicesViewModel
    {
        public static string[] Fields = {
            "Id","Name","Price","Name","Height","Width","Length","Description","Type","Unit","Tooltip","ImageUrl"
        };
        [BindNever]
        public Guid? Id { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public decimal? Price { get; set; }
        [BindNever]
        public decimal? Height { get; set; }
        [BindNever]
        public decimal? Width { get; set; }
        [BindNever]
        public decimal? Length { get; set; }
        [BindNever]
        public string Description { get; set; }
        public int? Type { get; set; }
        [BindNever]
        public string Unit { get; set; }
        [BindNever]
        public string Tooltip { get; set; }
        [BindNever]
        public string ImageUrl { get; set; }
    }
}
