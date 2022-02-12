using System;

namespace RSSMS.DataService.ViewModels.OrderHistoryExtension
{
    public class OrderHistoryExtensionViewModel
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? RequestId { get; set; }
        public DateTime? OldReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Status { get; set; }
        public string Note { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
