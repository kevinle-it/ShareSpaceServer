using ShareSpaceServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShareSpaceServer.Controllers
{
    public class UserController : ApiController
    {
        [HttpGet]
        [Authorize]
        public bool GetDummy()
        {
            return true;
        }
    }
}
