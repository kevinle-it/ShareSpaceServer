using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class NoteModel
    {
        public int HousingID { get; set; }
        public String NoteName { get; set; }
        public String Content { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}