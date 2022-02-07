using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderHistoryExtension
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public DateTime? OldReturnDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? Status { get; set; }
        public string Note { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual Order Order { get; set; }
    }
}
