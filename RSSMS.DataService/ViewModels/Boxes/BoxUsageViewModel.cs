namespace RSSMS.DataService.ViewModels.Boxes
{
    public class BoxUsageViewModel
    {
        public string SizeType { get; set; }
        public double? Usage { get; set; }
        public int ProductType { get; set; }

        public int TotalBox { get; set; }
        public int BoxRemaining { get; set; }
    }
}
