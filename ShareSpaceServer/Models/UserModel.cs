using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public String UID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public String DOB { get; set; }
        public String PhoneNumber { get; set; }
        public String Gender { get; set; }
        public int NumOfNote { get; set; }
        public String DeviceTokens { get; set; }
    }
}