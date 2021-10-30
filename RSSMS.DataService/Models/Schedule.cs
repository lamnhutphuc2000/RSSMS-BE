using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Schedule
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? UserId { get; set; }
        public int? RequestId { get; set; }
        public string Address { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual Order Order { get; set; }
        public virtual Request Request { get; set; }
        public virtual User User { get; set; }
    }
}
