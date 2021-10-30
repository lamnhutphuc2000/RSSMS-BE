#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderStorageDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int StorageId { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order Order { get; set; }
        public virtual Storage Storage { get; set; }
    }
}
