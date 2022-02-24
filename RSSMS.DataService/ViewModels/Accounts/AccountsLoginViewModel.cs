using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountsLoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceToken { get; set; }
    }
}
