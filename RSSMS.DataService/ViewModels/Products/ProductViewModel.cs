namespace RSSMS.DataService.ViewModels.Products
{
    class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int? Unit { get; set; }
        public string Description { get; set; }
        public int? Tooltip { get; set; }
        public int? Type { get; set; }

    }
}
