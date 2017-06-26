using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ShareSpaceServer.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ShareSpaceServer.MessageHandlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        public async Task<Dictionary<string, X509Certificate2>> FetchGooglePublicKeys()
        {
            using (var http = new HttpClient())
            {
                var response = await http.GetAsync("https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com");

                var dictionary = await response.Content.ReadAsAsync<Dictionary<string, string>>();
                return dictionary.ToDictionary(k => k.Key, k => new X509Certificate2(Encoding.UTF8.GetBytes(k.Value)));
            }
        }

        public async Task<ClaimsPrincipal> ValidateToken(string token)
        {
            var publicKeys = await FetchGooglePublicKeys();

            try
            {
                TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/" + Constants.CLIENT_ID,

                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    //IssuerSigningKeys = publicKeys.Values.Select(k => new X509SecurityKey(k)),
                    IssuerSigningKeyResolver = (local_token, securityToken, kid, validationParameters) =>
                    {
                        return publicKeys.Where(k => k.Key.ToUpper() == kid.ToUpper())
                                        .Select(k => new X509SecurityKey(k.Value));
                    },

                    ValidateAudience = true,
                    ValidAudience = Constants.CLIENT_ID,

                    ValidateLifetime = true
                };

                JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                ClaimsPrincipal claimsPrincipal = jwt.ValidateToken(token, tokenValidationParameters, out validatedToken);

                return claimsPrincipal;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return null;
            }
        }

        //Method to validate credentials from Authorization
        //header value
        private async Task<Tuple<bool, ClaimsPrincipal>> ValidateCredentials(AuthenticationHeaderValue authenticationHeaderVal)
        {
            try
            {
                if (authenticationHeaderVal != null
                    && authenticationHeaderVal.Scheme == "Bearer"
                    && !String.IsNullOrEmpty(authenticationHeaderVal.Parameter))
                {
                    ClaimsPrincipal claimsPrincipal = await ValidateToken(authenticationHeaderVal.Parameter);
                    if (claimsPrincipal == null || claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated)
                    {
                        return new Tuple<bool, ClaimsPrincipal>(false, null);
                    }
                    return new Tuple<bool, ClaimsPrincipal>(true, claimsPrincipal);
                }
                return new Tuple<bool, ClaimsPrincipal>(false, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return new Tuple<bool, ClaimsPrincipal>(false, null);
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tuple<bool, ClaimsPrincipal> validatedResult = await ValidateCredentials(request.Headers.Authorization);
            bool isValidToken = validatedResult.Item1;
            ClaimsPrincipal claimsPrincipal = validatedResult.Item2;

            if (claimsPrincipal != null)
            {
                // Get all claims from token.
                Dictionary<string, string> claimsDict = claimsPrincipal.Claims.ToDictionary(x => x.Type, x => x.Value);

                // Deserialize the JSON "firebase" claim to get user email.
                /* "firebase": {
                        "identities": {
                            "email": [
                                "lmtri1995@gmail.com"
                            ]
                        },
                        "sign_in_provider": "password"
                   }
                 */
                dynamic result = JsonConvert.DeserializeObject<dynamic>(claimsDict.FirstOrDefault(x => x.Key == "firebase").Value);
                string userEmail = result.identities.email[0];

                // Get User ID (issued by Firebase).
                string userID = claimsDict.FirstOrDefault(x => x.Key == "user_id").Value;

                if (isValidToken)
                {
                    SetPrincipal(new ShareSpaceAPIPrincipal(userEmail, userID));
                }
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(Constants.AUTHENTICATION_SCHEME));
                response.Headers.Add("Username", "null");
            }

            return response;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}