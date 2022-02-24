using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class StaffAssignStorage
    {
        public Guid Id { get; set; }
        public Guid? StorageId { get; set; }
        public Guid? StaffId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public string RoleName { get; set; }

        public virtual Account Staff { get; set; }
        public virtual Storage Storage { get; set; }
    }
}
