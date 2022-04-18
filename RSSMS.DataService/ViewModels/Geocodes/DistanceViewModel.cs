using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.ViewModels.Geocodes
{
    public class DistanceViewModel
    {
        public Row[] rows { get; set; }

        public class Rootobject
        {
            public Row[] rows { get; set; }
        }

        public class Row
        {
            public Element[] elements { get; set; }
        }

        public class Element
        {
            public string status { get; set; }
            public Duration duration { get; set; }
            public Distance distance { get; set; }
        }

        public class Duration
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class Distance
        {
            public string text { get; set; }
            public int value { get; set; }
        }

    }
}
