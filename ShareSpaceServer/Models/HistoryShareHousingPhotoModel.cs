using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class HistoryShareHousingPhotoModel
    {
        public ShareHousingModel ShareHousing { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}