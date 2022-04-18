
namespace RSSMS.DataService.ViewModels.Geocodes
{
    public class GeometryViewModel
    {
        public Plus_Code plus_code { get; set; }
        public Result[] results { get; set; }
        public string status { get; set; }

        public class Rootobject
        {
            public Plus_Code plus_code { get; set; }
            public Result[] results { get; set; }
            public string status { get; set; }
        }

        public class Plus_Code
        {
        }

        public class Result
        {
            public Address_Components[] address_components { get; set; }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public string place_id { get; set; }
            public string reference { get; set; }
            public Plus_Code1 plus_code { get; set; }
            public object[] types { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
        }

        public class Location
        {
            public float lat { get; set; }
            public float lng { get; set; }
        }

        public class Plus_Code1
        {
            public string compound_code { get; set; }
            public string global_code { get; set; }
        }

        public class Address_Components
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
        }

    }
}
