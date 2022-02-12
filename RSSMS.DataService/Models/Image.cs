using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Image
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string Note { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public int? StorageId { get; set; }
        public int? OrderId { get; set; }
        public string Name { get; set; }
        public int? OrderDetailId { get; set; }

        public virtual Order Order { get; set; }
        public virtual OrderDetail OrderDetail { get; set; }
        public virtual Product Product { get; set; }
        public virtual Storage Storage { get; set; }
        public virtual User User { get; set; }
    }
}
