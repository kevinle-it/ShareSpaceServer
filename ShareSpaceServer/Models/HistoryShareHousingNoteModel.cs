using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class HistoryShareHousingNoteModel
    {
        public ShareHousingModel ShareHousing { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}