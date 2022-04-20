using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Imports
{
    public class ImportViewModel
    {
        public Guid Id { get; set; }
        public Guid? FloorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Code { get; set; }
        public Guid? DeliveryBy { get; set; }
    }
}
