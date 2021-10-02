using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.JWT
{
    public class TokenGenerateModel
    {
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public double ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}
