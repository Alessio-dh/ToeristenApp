using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToeristenApp
{
    public class GooglePlacesResponse
    {
        public string status { get; set; }
        public results[] results { get; set; }
    }
}
