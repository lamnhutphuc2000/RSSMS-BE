#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderBoxDetail
    {
        public int OrderId { get; set; }
        public int BoxId { get; set; }

        public virtual Box Box { get; set; }
        public virtual Order Order { get; set; }
    }
}
