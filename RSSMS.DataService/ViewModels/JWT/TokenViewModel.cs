﻿using RSSMS.DataService.ViewModels.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.JWT
{
    public class TokenViewModel
    {
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public double ExpiresIn { get; set; }
        public string LocalId { get; set; }
        public string TokenType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string RoleName { get; set; }
        public int? StorageId { get; set; }
        public string Phone { get; set; }
        public virtual ICollection<AvatarImageViewModel> Images { get; set; }

    }
}