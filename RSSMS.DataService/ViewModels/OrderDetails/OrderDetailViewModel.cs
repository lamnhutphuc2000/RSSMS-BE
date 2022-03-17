using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailViewModel
    {
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public virtual ICollection<ImageOrderDetailCreateViewModel> OrderDetailImages { get; set; }
        public virtual ICollection<OrderDetailServiceViewModel> OrderDetailServices { get; set; }
    }
}
