using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class OrderDetail
    {
        public OrderDetail()
        {
            Images = new HashSet<Image>();
            OrderDetailServiceMaps = new HashSet<OrderDetailServiceMap>();
            TransferDetails = new HashSet<TransferDetail>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public Guid? ImportId { get; set; }
        public string ImportNote { get; set; }
        public string ImportCode { get; set; }
        public Guid? ExportId { get; set; }
        public string ExportNote { get; set; }

        public virtual Export Export { get; set; }
        public virtual Import Import { get; set; }
        public virtual Order Order { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<OrderDetailServiceMap> OrderDetailServiceMaps { get; set; }
        public virtual ICollection<TransferDetail> TransferDetails { get; set; }
    }
}
