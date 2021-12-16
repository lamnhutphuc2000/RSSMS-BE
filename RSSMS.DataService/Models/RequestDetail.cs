#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class RequestDetail
    {
        public int RequestId { get; set; }
        public int BoxId { get; set; }

        public virtual Box Box { get; set; }
        public virtual Request Request { get; set; }
    }
}
