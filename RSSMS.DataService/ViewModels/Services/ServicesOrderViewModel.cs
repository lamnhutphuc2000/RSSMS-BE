using System;

namespace RSSMS.DataService.ViewModels.Services
{
    public class ServicesOrderViewModel
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal? Price { get; set; }
        public int? Type { get; set; }
        public int? Amount { get; set; }
        public string Note { get; set; }

    }
}
