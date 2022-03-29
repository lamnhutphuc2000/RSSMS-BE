using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailInFloorViewModel
    {
        public string OrderName { get; set; }
        public string CustomerName { get; set; }
        public int? OrderStatus { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public ICollection<AvatarImageViewModel> Images { get; set; }
    }
}
