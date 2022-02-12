#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderBoxDetail
    {
        public int OrderId { get; set; }
        public int BoxId { get; set; }
        public int Id { get; set; }
        public bool? IsActive { get; set; }
    }
}
