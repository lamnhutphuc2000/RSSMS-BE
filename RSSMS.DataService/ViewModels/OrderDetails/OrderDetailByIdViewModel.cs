﻿using RSSMS.DataService.ViewModels.BoxOrderDetails;
using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.OrderDetails
{
    public class OrderDetailByIdViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? Amount { get; set; }
        public int? ProductType { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }
        public virtual ICollection<AvatarImageViewModel> ProductImages { get; set; }

        public BoxOrderViewModel BoxDetails { get; set; }
    }
}
