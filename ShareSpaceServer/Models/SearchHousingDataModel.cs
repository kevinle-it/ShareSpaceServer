using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class SearchHousingDataModel
    {
        public decimal Latitude { get; set; }    // [-90, 90]
        public decimal Longitude { get; set; }   // [-180, 180)

        public int Radius { get; set; }     // In Meters. [0, 8] km = [0, 8000] m

        public string Keywords { get; set; }
        public bool[] HouseTypes { get; set; }    // [Any, Apartment, Private House, Street Front House, Private Room]

        public int MinPrice { get; set; }   // -2 is Any. -1 is Deal. [Any, Deal, 500k, 1000k, 3000k, 5000k, 10000k, 40000k]
        public int MaxPrice { get; set; }   // -2 is Any. -1 is Deal. [Any, Deal, 500k, 1000k, 3000k, 5000k, 10000k, 40000k]

        public decimal MinArea { get; set; }  // -1 is Any. [Any, 30m, 50m, 80m, 100m, 150m, 200m, 250m, 300m, 500m]
        public decimal MaxArea { get; set; }  // -1 is Any. [Any, 30m, 50m, 80m, 100m, 150m, 200m, 250m, 300m, 500m]
        public int NumPeople { get; set; }  // -1 is Any. [Any, 1+, 2+, 3+, 4+, 5+]
        public int NumRoom { get; set; }    // -1 is Any. [Any, 1+, 2+, 3+, 4+, 5+]
        public int NumBed { get; set; }     // -1 is Any. [Any, 1+, 2+, 3+, 4+, 5+]
        public int NumBath { get; set; }    // -1 is Any. [Any, 1+, 2+, 3+, 4+, 5+]

        public bool[] Amenities { get; set; }           // [Any, Allow Pet, Has Wifi, Has AC, Has Parking]

        public string MinTimeRestriction { get; set; }   // "" is Any. [00:00, 23:59]
        public string MaxTimeRestriction { get; set; }   // "" is Any. [00:00, 23:59]
    }
}