﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Users
{
    public class UserChangePasswordViewModel
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
