namespace RSSMS.DataService.ViewModels.Users
{
    public class UserCreateThirdPartyViewModel
    {

        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public string Phone { get; set; }
        public int? RoleId { get; set; }    
        public string DeviceToken { get; set; }

        public UserCreateThirdPartyViewModel(string name, string password, string email,  string image, string phone, string deviceToken)
        {
            Name = name;
            Password = password;
            Email = email;
            Image = image;
            Phone = phone;
            DeviceToken = deviceToken;
        }
    }
}
