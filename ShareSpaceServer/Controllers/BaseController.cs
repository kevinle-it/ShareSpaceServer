using ShareSpaceServer.DBMapping;
using ShareSpaceServer.Models;
using ShareSpaceServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;

namespace ShareSpaceServer.Controllers
{
    public abstract class BaseController : ApiController
    {
        protected UserModel GetCurrentUserInfo()
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                if (User as ShareSpaceAPIPrincipal != null)
                {
                    String email = (User as ShareSpaceAPIPrincipal).UserEmail;
                    String UID = (User as ShareSpaceAPIPrincipal).UserID;

                    MailAddress emailAddress = new MailAddress(email);

                    var emailDomain = (from ed in db.GetTable<EmailDomain>()
                                       where ed.DomainName == emailAddress.Host
                                       select ed).SingleOrDefault();
                    if (emailDomain != null)
                    {
                        var currentEmail = (from e in db.GetTable<Email>()
                                            where (e.DomainID == emailDomain.DomainID)
                                               && (e.LocalPart == emailAddress.User)
                                            select e).SingleOrDefault();
                        if (currentEmail != null)
                        {
                            var user = (from u in db.GetTable<ShareSpaceUser>()
                                        where u.EmailID == currentEmail.EmailID
                                        select u).SingleOrDefault();

                            if (String.Equals(UID, user.UID, StringComparison.InvariantCulture))
                            {
                                UserModel currentUser = new UserModel
                                {
                                    UserID = user.UserID,
                                    UID = user.UID,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Email = email,
                                    DOB = user.DOB,
                                    PhoneNumber = user.PhoneNumber,
                                    Gender = (from g in db.GetTable<Gender>()
                                              where g.GenderID == user.GenderID
                                              select g.GenderType).SingleOrDefault(),
                                    NumOfNote = user.NumOfNote,
                                    DeviceTokens = user.DeviceTokens
                                };
                                return currentUser;
                            }
                        }
                    }
                }                
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        protected DateTimeOffset? ParseDateFromStringOrNull(string dateTimeOffset)
        {
            if (dateTimeOffset != null)
            {
                return DateTimeOffset.ParseExact(
                        dateTimeOffset,
                        "yyyy-MM-ddTHH:mm:ss.fffzzzz",  // "2007-08-31T06:59:40.504+02:00"
                        // System.Globalization.CultureInfo.InvariantCulture
                        null
                    ).ToUniversalTime();
            }
            return null;
        }

        protected DateTimeOffset ParseDateFromString(string dateTimeOffset)
        {
            return DateTimeOffset.ParseExact(
                        dateTimeOffset,
                        "yyyy-MM-ddTHH:mm:ss.fffzzzz",  // "2007-08-31T06:59:40.504+02:00"
                        // System.Globalization.CultureInfo.InvariantCulture
                        null
                    ).ToUniversalTime();
        }
    }
}
