using ShareSpaceServer.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ShareSpaceServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Add Authentication Message Handler to authenticate every HTTP request to this Server.
            GlobalConfiguration.Configuration.MessageHandlers.Add(new AuthenticationHandler());
        }
    }
}
