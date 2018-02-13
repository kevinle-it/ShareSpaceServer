using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace ShareSpaceServer.Security
{
    public class ShareSpaceAPIPrincipal : IPrincipal
    {
        public ShareSpaceAPIPrincipal(string userEmail, string userID)
        {
            UserEmail = userEmail;
            UserID = userID;
            Identity = new GenericIdentity(userEmail);
        }

        public string UserEmail { get; set; }

        // UserID issued by Firebase.
        public string UserID { get; set; }

        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            if (role.Equals("user"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}