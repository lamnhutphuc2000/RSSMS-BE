using RSSMS.DataService.ViewModels.RequestDetail;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestCreateViewModel
    {
        public Guid? OrderId { get; set; }
        public decimal? TotalPrice { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryTime { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? OldReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? CancelDay { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsCustomerDelivery { get; set; }
        public int Type { get; set; }
        public string Note { get; set; }
        public List<RequestDetailCreateViewModel> RequestDetails { get; set; }
    }
}
