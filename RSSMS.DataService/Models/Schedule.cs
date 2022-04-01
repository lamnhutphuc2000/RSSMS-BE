using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Schedule
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? RequestId { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public int? Status { get; set; }
        public DateTime ScheduleDay { get; set; }
        public string ScheduleTime { get; set; }
        public bool IsActive { get; set; }
        public Guid? ModifiedBy { get; set; }

        public virtual Request Request { get; set; }
        public virtual Account User { get; set; }
    }
}
