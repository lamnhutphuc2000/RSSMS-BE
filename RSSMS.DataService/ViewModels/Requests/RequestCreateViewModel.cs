﻿using System;
using System.Collections.Generic;
using System.Text;
using RSSMS.DataService.Models;
using RSSMS.DataService.ViewModels.RequestDetails;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCreateViewModel
    {

        public int? OrderId { get; set; }
        public int? UserId { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public string Note { get; set; }

        public virtual ICollection<RequestDetailCreateViewModel> RequestDetails { get; set; }

    }
}