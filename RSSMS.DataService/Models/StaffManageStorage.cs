#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class StaffManageStorage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StorageId { get; set; }
        public string StorageName { get; set; }
        public string RoleName { get; set; }

        public virtual Storage Storage { get; set; }
        public virtual User User { get; set; }
    }
}
