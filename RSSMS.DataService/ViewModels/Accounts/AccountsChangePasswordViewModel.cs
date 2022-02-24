using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountsChangePasswordViewModel
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
