namespace RSSMS.DataService.ViewModels.Users
{
    public class UserCreateViewModel
    {
        public int? RoleId { get; set; }
        public int? StorageId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
