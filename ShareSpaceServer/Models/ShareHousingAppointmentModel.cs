using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class ShareHousingAppointmentModel
    {
        public ShareHousingModel ShareHousing { get; set; }
        public UserModel Sender { get; set; }   // User.
        public int RecipientID { get; set; }    // Owner.
        public DateTimeOffset AppointmentDateTime { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
        public String Content { get; set; }
        public bool IsOwnerConfirmed { get; set; }
        public bool IsUserConfirmed { get; set; }
        public int NumOfRequests { get; set; }
    }
}