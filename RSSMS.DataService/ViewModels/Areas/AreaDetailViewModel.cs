namespace RSSMS.DataService.ViewModels.Areas
{
    public class AreaDetailViewModel
    {
        public int Id { get; set; }
        public int? StorageId { get; set; }
        public string Name { get; set; }
        public double? Usage { get; set; }
        public int? Status { get; set; }

        public int TotalBox { get; set; }
        public int BoxRemaining { get; set; }
    }

}
