namespace RSSMS.DataService.ViewModels.JWT
{
    public class TokenGenerateViewModel
    {
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public double ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}
