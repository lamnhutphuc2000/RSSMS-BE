using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.RequestDetails;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestViewModel
    {
        public static string[] Fields = {
            "Id","OrderId","UserId","Type","Status","Note","RequestDetails"
        };
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public int? OrderId { get; set; }
        [BindNever]
        public int? UserId { get; set; }
        [BindNever]
        public int? Type { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public string Note { get; set; }
        [BindNever]
        public virtual ICollection<RequestDetailCreateViewModel> RequestDetails { get; set; }

    }
}
