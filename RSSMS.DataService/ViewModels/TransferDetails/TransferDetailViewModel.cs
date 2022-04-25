using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.TransferDetails
{
    public class TransferDetailViewModel
    {
        public string AreaFromName { get; set; }
        public string SpaceFromName { get; set; }
        public string FloorFromName { get; set; }
        public string AreaToName { get; set; }
        public string SpaceToName { get; set; }
        public string FloorToName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string StaffName { get; set; }
    }
}
