#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? Amount { get; set; }
        public double? TotalPrice { get; set; }
        public string Note { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
