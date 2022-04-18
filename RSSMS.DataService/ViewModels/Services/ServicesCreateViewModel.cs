using RSSMS.DataService.ViewModels.Images;

namespace RSSMS.DataService.ViewModels.Services
{
    public class ServicesCreateViewModel
    {
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public string Description { get; set; }
        public int? Type { get; set; }
        public string Unit { get; set; }
        public string Tooltip { get; set; }
        public decimal? DeliveryFee { get; set; }
        public ImageCreateViewModel Image { get; set; }
    }
}
