namespace RSSMS.DataService.ViewModels.Accounts
{
    public class AccountsCreateThirdPartyViewModel
    {
        public AccountsCreateThirdPartyViewModel(string name, string image, string phone, string deviceToken)
        {
            Name = name;
            Image = image;
            Phone = phone;
            DeviceToken = deviceToken;
        }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public string Phone { get; set; }
        public int? RoleId { get; set; }
        public string DeviceToken { get; set; }
    }
}
