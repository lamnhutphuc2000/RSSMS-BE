namespace RSSMS.DataService.ViewModels.Users
{
    public class UserUpdateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? StorageId { get; set; }
    }
}
