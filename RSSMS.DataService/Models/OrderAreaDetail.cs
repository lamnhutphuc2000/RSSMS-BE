#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderAreaDetail
    {
        public int? OrderId { get; set; }
        public int? AreaId { get; set; }
        public int Id { get; set; }
        public bool? IsActive { get; set; }

        public virtual Area Area { get; set; }
        public virtual Order Order { get; set; }
    }
}
