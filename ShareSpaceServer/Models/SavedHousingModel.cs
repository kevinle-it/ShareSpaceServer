using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class SavedHousingModel
    {
        public HousingModel SavedHousing { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}