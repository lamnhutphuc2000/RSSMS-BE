using RSSMS.DataService.ViewModels.Images;
using System;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountUpdateViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public ImageCreateViewModel Image { get; set; }
    }
}
