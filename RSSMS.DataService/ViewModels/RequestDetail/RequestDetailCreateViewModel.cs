﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.RequestDetail
{
    public class RequestDetailCreateViewModel
    {
        public Guid ServiceId { get; set; }
        public int Amount { get; set; }
        public float Price{ get; set; }
        public string Note { get; set; }
    }
}