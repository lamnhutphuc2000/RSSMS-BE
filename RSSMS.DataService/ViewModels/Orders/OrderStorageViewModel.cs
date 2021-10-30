namespace RSSMS.DataService.ViewModels.Orders
{
    public partial class OrderStorageViewModel
    {
        public static string[] Fields = {
            "Id","CustomerId","CustomerName","CustomerPhone","DurationDays","DurationMonths"};
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public int? DurationDays { get; set; }
        public int? DurationMonths { get; set; }

    }
}
