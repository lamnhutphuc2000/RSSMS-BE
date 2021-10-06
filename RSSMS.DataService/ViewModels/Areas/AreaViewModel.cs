﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RSSMS.DataService.ViewModels.Areas
{
    public partial class  AreaViewModel
    {

        public static string[] Fields = {
            "Name","Usage","Status","IsActive"};

        [BindNever]
        public string Name { get; set; }
        [BindNever]
        public int? Usage { get; set; }
        [BindNever]
        public int? Status { get; set; }
        [BindNever]
        public bool? IsActive { get; set; }



    }
}
