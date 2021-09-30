using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Image
    {
        public int Id { get; set; }
        public int? ResourceId { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string Note { get; set; }

        public virtual Product Resource { get; set; }
        public virtual Storage Resource1 { get; set; }
        public virtual User Resource2 { get; set; }
        public virtual Request ResourceNavigation { get; set; }
    }
}
