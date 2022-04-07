using System;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderAdditionalFee
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public int? Type { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }

        public virtual Order Order { get; set; }
    }
}
