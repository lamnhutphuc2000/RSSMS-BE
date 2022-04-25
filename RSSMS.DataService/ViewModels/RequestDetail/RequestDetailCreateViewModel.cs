using System;

namespace RSSMS.DataService.ViewModels.RequestDetail
{
    public class RequestDetailCreateViewModel
    {
        public Guid ServiceId { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
    }
}
