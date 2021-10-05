using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Storage
    {
        public Storage()
        {
            Areas = new HashSet<Area>();
            Images = new HashSet<Image>();
            OrderStorageDetails = new HashSet<OrderStorageDetail>();
            StaffManageStorages = new HashSet<StaffManageStorage>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public int? Usage { get; set; }
        public int? Type { get; set; }
        public string Address { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public int? OrderId { get; set; }

        public virtual ICollection<Area> Areas { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<OrderStorageDetail> OrderStorageDetails { get; set; }
        public virtual ICollection<StaffManageStorage> StaffManageStorages { get; set; }
    }
}
