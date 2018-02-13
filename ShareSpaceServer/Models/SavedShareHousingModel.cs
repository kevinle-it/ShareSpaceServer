using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class SavedShareHousingModel
    {
        public ShareHousingModel SavedShareHousing { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}