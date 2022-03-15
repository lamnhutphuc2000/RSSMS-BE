using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace RSSMS.DataService.ViewModels.Areas
{
    public partial class AreaViewModel
    {

        public static string[] Fields = {
            "Id","StorageId","Name","Height","Width","Length","Type","Status","Description","Usage"};
        [BindNever]
        public Guid? Id { get; set; }
        [BindNever]
        public Guid? StorageId { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public decimal? Height { get; set; }
        [BindNever]
        public decimal? Width { get; set; }
        [BindNever]
        public decimal? Length { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public string Description { get; set; }
        [BindNever]
        public int? Usage { get; set; }


    }
}
