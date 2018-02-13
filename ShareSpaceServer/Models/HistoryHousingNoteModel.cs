using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class HistoryHousingNoteModel
    {
        public HousingModel Housing { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}