using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RSSMS.DataService.ViewModels.Areas
{
    public partial class AreaViewModel
    {

        public static string[] Fields = {
            "Id","Name","Usage","Status","Description","IsActive"};
        [BindNever]
        public int? Id { get; set; }
        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public string Description { get; set; }
        [BindNever]
        public int? Usage { get; set; }
        [BindNever]
        public int? Status { get; set; }

    }
}
