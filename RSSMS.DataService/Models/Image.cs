using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Image
    {
        public Guid Id { get; set; }
        public Guid? OrderDetailid { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public bool? IsActive { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
    }
}
