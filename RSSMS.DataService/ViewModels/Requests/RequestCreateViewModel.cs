﻿namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCreateViewModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
    }
}
