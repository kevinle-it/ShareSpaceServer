using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace ShareSpaceServer.Security
{
    public class ShareSpaceAPIPrincipal : IPrincipal
    {
        public ShareSpaceAPIPrincipal(string username, string userID)
        {
            Username = username;
            UserID = userID;
            Identity = new GenericIdentity(username);
        }

        public string Username { get; set; }

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