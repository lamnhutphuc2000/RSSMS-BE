using System;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountChangePasswordViewModel
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
