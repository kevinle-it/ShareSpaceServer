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
    [RoutePrefix("api/user")]
    public class UserController : BaseController
    {
        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpPost]
        [Authorize]
        public UserModel Register(UserModel user)
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                String email = (User as ShareSpaceAPIPrincipal).UserEmail;
                String UID = (User as ShareSpaceAPIPrincipal).UserID;

                MailAddress emailAddress = new MailAddress(email);

                var oldEmailDomain = (from ed in db.GetTable<EmailDomain>()
                                   where ed.DomainName == emailAddress.Host
                                   select ed).SingleOrDefault();
                if (oldEmailDomain != null)
                {
                    // No need to check for duplicate email
                    // Because the business was handled by Firebase Authentication.
                    Email newEmail = new Email
                    {
                        DomainID = oldEmailDomain.DomainID,
                        LocalPart = emailAddress.User
                    };
                    db.Emails.InsertOnSubmit(newEmail);
                    db.SubmitChanges();

                    ShareSpaceUser newUser = new ShareSpaceUser
                    {
                        UID = UID,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        GenderID = (from g in db.GetTable<Gender>()
                                    where g.GenderType == user.Gender
                                    select g.GenderID).SingleOrDefault(),
                        DOB = user.DOB,
                        EmailID = newEmail.EmailID,
                        PhoneNumber = user.PhoneNumber,
                        AddressID = null,
                        SchoolID = null,
                        StartSchoolYear = null,
                        WorkID = null,
                        HometownID = null,
                        Description = "",
                        NumOfNote = user.NumOfNote,
                        DateTimeCreated = DateTime.UtcNow,
                        DeviceTokens = user.DeviceTokens
                    };
                    db.ShareSpaceUsers.InsertOnSubmit(newUser);
                    db.SubmitChanges();

                    user.UserID = newUser.UserID;
                    return user;
                }
                else
                {                    
                    EmailDomain newEmailDomain = new EmailDomain
                    {
                        DomainName = emailAddress.Host
                    };
                    db.EmailDomains.InsertOnSubmit(newEmailDomain);
                    db.SubmitChanges();

                    Email newEmail = new Email
                    {
                        DomainID = newEmailDomain.DomainID,
                        LocalPart = emailAddress.User
                    };
                    db.Emails.InsertOnSubmit(newEmail);
                    db.SubmitChanges();

                    ShareSpaceUser newUser = new ShareSpaceUser
                    {
                        UID = UID,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        GenderID = (from g in db.GetTable<Gender>()
                                    where g.GenderType == user.Gender
                                    select g.GenderID).SingleOrDefault(),
                        DOB = user.DOB,
                        EmailID = newEmail.EmailID,
                        PhoneNumber = user.PhoneNumber,
                        AddressID = null,
                        SchoolID = null,
                        StartSchoolYear = null,
                        WorkID = null,
                        HometownID = null,
                        Description = "",
                        NumOfNote = user.NumOfNote,
                        DateTimeCreated = DateTime.UtcNow,
                        DeviceTokens = user.DeviceTokens
                    };
                    db.ShareSpaceUsers.InsertOnSubmit(newUser);
                    db.SubmitChanges();

                    user.UserID = newUser.UserID;
                    return user;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        [Authorize]
        public UserModel GetUserInfo(string currentDeviceToken)
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

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
                            if (user.DeviceTokens != null)
                            {
                                string[] tokens = user.DeviceTokens.Split(';');
                                bool isExist = false;
                                for (int i = 0; i < tokens.Length; ++i)
                                {
                                    if (tokens[i].Equals(currentDeviceToken))
                                    {
                                        isExist = true;
                                        break;
                                    }
                                }
                                if (!isExist)
                                {
                                    user.DeviceTokens = user.DeviceTokens + ";" + currentDeviceToken;
                                    db.SubmitChanges();
                                }
                                UserModel currentUser = new UserModel
                                {
                                    UserID = user.UserID,
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
                            else
                            {
                                user.DeviceTokens = currentDeviceToken;
                                db.SubmitChanges();

                                UserModel currentUser = new UserModel
                                {
                                    UserID = user.UserID,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Email = email,
                                    DOB = user.DOB,
                                    PhoneNumber = user.PhoneNumber,
                                    Gender = (from g in db.GetTable<Gender>()
                                              where g.GenderID == user.GenderID
                                              select g.GenderType).SingleOrDefault(),
                                    NumOfNote = user.NumOfNote,
                                    DeviceTokens = currentDeviceToken
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

        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        [Authorize]
        public UserModel GetOtherUserInfo(int userID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    return (from u in db.GetTable<ShareSpaceUser>()
                            where u.UserID == userID
                            select new UserModel
                            {
                                UserID = u.UserID,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Email = (from user in db.GetTable<ShareSpaceUser>()
                                         join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                         where user.UserID == userID
                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                DOB = u.DOB,
                                PhoneNumber = u.PhoneNumber,
                                Gender = (from user in db.GetTable<ShareSpaceUser>()
                                          join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                          where user.UserID == userID
                                          select g.GenderType).SingleOrDefault(),
                                NumOfNote = u.NumOfNote,
                                DeviceTokens = u.DeviceTokens
                            }).SingleOrDefault();
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/housing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfSavedHousings(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var savedHousings = (from s in db.GetTable<SavedHousing>()
                                         where s.CreatorID == currentUser.UserID
                                         select s).ToList();
                    if (savedHousings != null && savedHousings.Count > 0)
                    {
                        return savedHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/housing")]
        [HttpGet]
        [Authorize]
        public List<SavedHousingModel> GetMoreOlderSavedHousings(string currentBottomSavedHousingDateTimeCreated = null)  //, int offset)
        {
            try
            {
                List<SavedHousingModel> olderSavedHousings = new List<SavedHousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomSavedHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderSavedHousings =
                            (from h in db.GetTable<Housing>()
                             join sh in db.GetTable<SavedHousing>() on h.HousingID equals sh.HousingID
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (sh.DateTimeCreated < parsedDate)
                                && (sh.CreatorID == currentUser.UserID)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby sh.DateTimeCreated descending
                             select new SavedHousingModel
                             {
                                 SavedHousing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = sh.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastSavedHousing =
                            (from h in db.GetTable<Housing>()
                             join sh in db.GetTable<SavedHousing>() on h.HousingID equals sh.HousingID
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (sh.CreatorID == currentUser.UserID)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby sh.DateTimeCreated descending
                             select new SavedHousingModel
                             {
                                 SavedHousing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = sh.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();

                        if (lastSavedHousing != null)
                        {
                            olderSavedHousings.Add(lastSavedHousing);
                        }
                    }
                    if (olderSavedHousings != null && olderSavedHousings.Count > 0)
                    {
                        foreach (var item in olderSavedHousings)
                        {
                            item.SavedHousing.PhotoURLs = (from a in db.GetTable<Album>()
                                                      join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                      where (a.HousingID == item.SavedHousing.ID)
                                                         && (a.CreatorID == item.SavedHousing.Owner.UserID)
                                                      orderby p.PhotoID
                                                      select p.PhotoLink).ToList();
                        }
                    }
                }
                return olderSavedHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/housing")]
        [HttpGet]
        [Authorize]
        public List<SavedHousingModel> GetMoreNewerSavedHousings(string currentTopSavedHousingDateTimeCreated)
        {
            try
            {
                List<SavedHousingModel> newerSavedHousings = new List<SavedHousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset parsedDate = base.ParseDateFromString(currentTopSavedHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    newerSavedHousings =
                        (from h in db.GetTable<Housing>()
                         join sh in db.GetTable<SavedHousing>() on h.HousingID equals sh.HousingID
                         join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                         join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                         join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                         join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                         join c in db.GetTable<City>() on a.CityID equals c.CityID
                         //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                         where (sh.DateTimeCreated > parsedDate)
                                && (sh.CreatorID == currentUser.UserID)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                         orderby sh.DateTimeCreated descending
                         select new SavedHousingModel
                         {
                             SavedHousing = new HousingModel
                             {
                                 ID = h.HousingID,
                                 Title = h.Title,
                                 Owner = new UserModel
                                 {
                                     UserID = h.OwnerID,
                                     UID = u.UID,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == h.OwnerID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u.DOB,
                                     PhoneNumber = u.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == h.OwnerID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u.NumOfNote,
                                     DeviceTokens = u.DeviceTokens
                                 },
                                 Price = h.Price,
                                 IsAvailable = h.IsAvailable,
                                 HouseType = ht.HousingType,
                                 DateTimeCreated = h.DateTimeCreated,
                                 NumOfView = h.NumOfView,
                                 NumOfSaved = h.NumOfSaved,
                                 NumOfPeople = h.NumOfPeople,
                                 NumOfRoom = h.NumOfRoom,
                                 NumOfBed = h.NumOfBed,
                                 NumOfBath = h.NumOfBath,
                                 AllowPet = h.AllowPet,
                                 HasWifi = h.HasWifi,
                                 HasAC = h.HasAC,
                                 HasParking = h.HasParking,
                                 TimeRestriction = h.TimeRestriction,
                                 Area = h.Area,
                                 Latitude = gl.Latitude,
                                 Longitude = gl.Longitude,
                                 AddressHouseNumber = a.HouseNumber,
                                 AddressStreet = a.Street,
                                 AddressWard = a.Ward,
                                 AddressDistrict = a.District,
                                 AddressCity = c.CityName,
                                 Description = h.Description,
                                 //LatestCommentContent = "",
                                 NumOfComment = h.NumOfComment,
                                 //AuthorizationValue = nameFilter
                             },
                             DateTimeCreated = sh.DateTimeCreated
                         }).Take(5).ToList();

                    if (newerSavedHousings != null && newerSavedHousings.Count > 0)
                    {
                        foreach (var item in newerSavedHousings)
                        {
                            item.SavedHousing.PhotoURLs = (from a in db.GetTable<Album>()
                                                      join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                      where (a.HousingID == item.SavedHousing.ID)
                                                         && (a.CreatorID == item.SavedHousing.Owner.UserID)
                                                      orderby p.PhotoID
                                                      select p.PhotoLink).ToList();
                        }
                    }
                }
                return newerSavedHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/housing")]
        [HttpGet]
        [Authorize]
        public bool GetSavingStateOfCurrentHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == housingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        var oldSavedHousing = (from s in db.GetTable<SavedHousing>()
                                               where (s.HousingID == currentHousing.HousingID)
                                                  && (s.CreatorID == currentUser.UserID)
                                               select s).SingleOrDefault();
                        if (oldSavedHousing != null)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/housing")]
        [HttpPost]
        [Authorize]
        public SavedHousingModel SaveHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == housingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == currentHousing.OwnerID
                                                   select u).SingleOrDefault();
                        var oldAddress = (from a in db.GetTable<Address>()
                                          join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                          join c in db.GetTable<City>() on a.CityID equals c.CityID
                                          where a.AddressID == currentHousing.AddressID
                                          select new { a, gl, c }).SingleOrDefault();
                        if (currentHousingOwner != null && oldAddress != null)
                        {
                            var oldSavedHousing = (from s in db.GetTable<SavedHousing>()
                                                   where (s.HousingID == currentHousing.HousingID)
                                                      && (s.CreatorID == currentUser.UserID)
                                                   select s).SingleOrDefault();
                            if (oldSavedHousing == null)
                            {
                                ++currentHousing.NumOfSaved;

                                SavedHousing newSavedHousing = new SavedHousing
                                {
                                    HousingID = currentHousing.HousingID,
                                    CreatorID = currentUser.UserID,
                                    DateTimeCreated = DateTimeOffset.UtcNow
                                };
                                db.SavedHousings.InsertOnSubmit(newSavedHousing);
                                db.SubmitChanges();
                                return new SavedHousingModel
                                {
                                    SavedHousing = new HousingModel
                                    {
                                        ID = currentHousing.HousingID,
                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                     where (a.HousingID == currentHousing.HousingID)
                                                        && (a.CreatorID == currentHousing.OwnerID)
                                                     select p.PhotoLink).ToList(),
                                        Title = currentHousing.Title,
                                        Owner = new UserModel
                                        {
                                            UserID = currentHousing.OwnerID,
                                            FirstName = currentHousingOwner.FirstName,
                                            LastName = currentHousingOwner.LastName,
                                            Email = (from e in db.GetTable<Email>()
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where e.EmailID == currentHousingOwner.EmailID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentHousingOwner.DOB,
                                            PhoneNumber = currentHousingOwner.PhoneNumber,
                                            Gender = (from g in db.GetTable<Gender>()
                                                      where g.GenderID == currentHousingOwner.GenderID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentHousingOwner.NumOfNote,
                                            DeviceTokens = currentHousingOwner.DeviceTokens
                                        },
                                        Price = currentHousing.Price,
                                        IsAvailable = currentHousing.IsAvailable,
                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                     where ht.HouseTypeID == currentHousing.HouseTypeID
                                                     select ht.HousingType).SingleOrDefault(),
                                        DateTimeCreated = currentHousing.DateTimeCreated,
                                        NumOfView = currentHousing.NumOfView,
                                        NumOfSaved = currentHousing.NumOfSaved,
                                        NumOfPeople = currentHousing.NumOfPeople,
                                        NumOfRoom = currentHousing.NumOfRoom,
                                        NumOfBed = currentHousing.NumOfBed,
                                        NumOfBath = currentHousing.NumOfBath,
                                        AllowPet = currentHousing.AllowPet,
                                        HasWifi = currentHousing.HasWifi,
                                        HasAC = currentHousing.HasAC,
                                        HasParking = currentHousing.HasParking,
                                        TimeRestriction = currentHousing.TimeRestriction,
                                        Area = currentHousing.Area,
                                        Latitude = oldAddress.gl.Latitude,
                                        Longitude = oldAddress.gl.Longitude,
                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                        AddressStreet = oldAddress.a.Street,
                                        AddressWard = oldAddress.a.Ward,
                                        AddressDistrict = oldAddress.a.District,
                                        AddressCity = oldAddress.c.CityName,
                                        Description = currentHousing.Description,
                                        NumOfComment = currentHousing.NumOfComment
                                    },
                                    DateTimeCreated = newSavedHousing.DateTimeCreated
                                };
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

        [Route("save/housing")]
        [HttpDelete]
        [Authorize]
        public SavedHousingModel UnsaveHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == housingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == currentHousing.OwnerID
                                                   select u).SingleOrDefault();
                        var oldAddress = (from a in db.GetTable<Address>()
                                          join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                          join c in db.GetTable<City>() on a.CityID equals c.CityID
                                          where a.AddressID == currentHousing.AddressID
                                          select new { a, gl, c }).SingleOrDefault();
                        if (currentHousingOwner != null && oldAddress != null)
                        {
                            var oldSavedHousing = (from s in db.GetTable<SavedHousing>()
                                                   where (s.HousingID == currentHousing.HousingID)
                                                      && (s.CreatorID == currentUser.UserID)
                                                   select s).SingleOrDefault();
                            if (oldSavedHousing != null)
                            {
                                --currentHousing.NumOfSaved;
                                db.SubmitChanges();

                                SavedHousingModel sh = new SavedHousingModel
                                {
                                    SavedHousing = new HousingModel
                                    {
                                        ID = currentHousing.HousingID,
                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                     where (a.HousingID == currentHousing.HousingID)
                                                        && (a.CreatorID == currentHousing.OwnerID)
                                                     select p.PhotoLink).ToList(),
                                        Title = currentHousing.Title,
                                        Owner = new UserModel
                                        {
                                            UserID = currentHousing.OwnerID,
                                            FirstName = currentHousingOwner.FirstName,
                                            LastName = currentHousingOwner.LastName,
                                            Email = (from e in db.GetTable<Email>()
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where e.EmailID == currentHousingOwner.EmailID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentHousingOwner.DOB,
                                            PhoneNumber = currentHousingOwner.PhoneNumber,
                                            Gender = (from g in db.GetTable<Gender>()
                                                      where g.GenderID == currentHousingOwner.GenderID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentHousingOwner.NumOfNote,
                                            DeviceTokens = currentHousingOwner.DeviceTokens
                                        },
                                        Price = currentHousing.Price,
                                        IsAvailable = currentHousing.IsAvailable,
                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                     where ht.HouseTypeID == currentHousing.HouseTypeID
                                                     select ht.HousingType).SingleOrDefault(),
                                        DateTimeCreated = currentHousing.DateTimeCreated,
                                        NumOfView = currentHousing.NumOfView,
                                        NumOfSaved = currentHousing.NumOfSaved,
                                        NumOfPeople = currentHousing.NumOfPeople,
                                        NumOfRoom = currentHousing.NumOfRoom,
                                        NumOfBed = currentHousing.NumOfBed,
                                        NumOfBath = currentHousing.NumOfBath,
                                        AllowPet = currentHousing.AllowPet,
                                        HasWifi = currentHousing.HasWifi,
                                        HasAC = currentHousing.HasAC,
                                        HasParking = currentHousing.HasParking,
                                        TimeRestriction = currentHousing.TimeRestriction,
                                        Area = currentHousing.Area,
                                        Latitude = oldAddress.gl.Latitude,
                                        Longitude = oldAddress.gl.Longitude,
                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                        AddressStreet = oldAddress.a.Street,
                                        AddressWard = oldAddress.a.Ward,
                                        AddressDistrict = oldAddress.a.District,
                                        AddressCity = oldAddress.c.CityName,
                                        Description = currentHousing.Description,
                                        NumOfComment = currentHousing.NumOfComment
                                    },
                                    DateTimeCreated = oldSavedHousing.DateTimeCreated
                                };
                                db.SavedHousings.DeleteOnSubmit(oldSavedHousing);
                                db.SubmitChanges();
                                return sh;
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

        [Route("save/sharehousing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfSavedShareHousings(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var savedShareHousings = (from s in db.GetTable<SavedShareHousing>()
                                              where s.CreatorID == currentUser.UserID
                                              select s).ToList();
                    if (savedShareHousings != null && savedShareHousings.Count > 0)
                    {
                        return savedShareHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<SavedShareHousingModel> GetMoreOlderSavedShareHousings(string currentBottomSavedShareHousingDateTimeCreated = null)   //, int offset)
        {
            try
            {
                List<SavedShareHousingModel> olderSavedShareHousings = new List<SavedShareHousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomSavedShareHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (currentBottomSavedShareHousingDateTimeCreated != null)
                    {
                        olderSavedShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join ssh in db.GetTable<SavedShareHousing>() on sh.ShareHousingID equals ssh.ShareHousingID
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (ssh.DateTimeCreated < parsedDate)
                                && (ssh.CreatorID == currentUser.UserID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ssh.DateTimeCreated descending
                             select new SavedShareHousingModel
                             {
                                 SavedShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = ssh.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastSavedShareHousing =
                            (from sh in db.GetTable<ShareHousing>()
                             join ssh in db.GetTable<SavedShareHousing>() on sh.ShareHousingID equals ssh.ShareHousingID
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (ssh.CreatorID == currentUser.UserID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ssh.DateTimeCreated descending
                             select new SavedShareHousingModel
                             {
                                 SavedShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = ssh.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();
                        if (lastSavedShareHousing != null)
                        {
                            olderSavedShareHousings.Add(lastSavedShareHousing);
                        }
                    }
                    if (olderSavedShareHousings != null && olderSavedShareHousings.Count > 0)
                    {
                        foreach (var item in olderSavedShareHousings)
                        {
                            item.SavedShareHousing.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                                   join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                   where (a.HousingID == item.SavedShareHousing.Housing.ID)
                                                                      && (a.CreatorID == item.SavedShareHousing.Housing.Owner.UserID)
                                                                   orderby p.PhotoID
                                                                   select p.PhotoLink).ToList();
                        }
                    }
                }
                return olderSavedShareHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<SavedShareHousingModel> GetMoreNewerSavedShareHousings(string currentTopSavedShareHousingDateTimeCreated)
        {
            try
            {
                List<SavedShareHousingModel> newerSavedShareHousings = new List<SavedShareHousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset parsedDate = base.ParseDateFromString(currentTopSavedShareHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    newerSavedShareHousings =
                        (from sh in db.GetTable<ShareHousing>()
                         join ssh in db.GetTable<SavedShareHousing>() on sh.ShareHousingID equals ssh.ShareHousingID
                         join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                         join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                         join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                         join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                         join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                         join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                         join c in db.GetTable<City>() on a.CityID equals c.CityID
                         //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                         where (ssh.DateTimeCreated > parsedDate)
                            && (ssh.CreatorID == currentUser.UserID)
                            && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                            && (h.IsAvailable == true)
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                         orderby ssh.DateTimeCreated descending
                         select new SavedShareHousingModel
                         {
                             SavedShareHousing = new ShareHousingModel
                             {
                                 ID = sh.ShareHousingID,
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u1.UID,
                                         FirstName = u1.FirstName,
                                         LastName = u1.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u1.DOB,
                                         PhoneNumber = u1.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u1.NumOfNote,
                                         DeviceTokens = u1.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     NumOfComment = h.NumOfComment
                                 },
                                 Creator = new UserModel
                                 {
                                     UserID = sh.CreatorID,
                                     FirstName = u2.FirstName,
                                     LastName = u2.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == sh.CreatorID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u2.DOB,
                                     PhoneNumber = u2.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == sh.CreatorID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u2.NumOfNote,
                                     DeviceTokens = u2.DeviceTokens
                                 },
                                 IsAvailable = sh.IsAvailable,
                                 PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                 Description = sh.Description,
                                 NumOfView = sh.NumOfView,
                                 NumOfSaved = sh.NumOfSaved,
                                 RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                 RequiredGender = (from g in db.GetTable<Gender>()
                                                   where g.GenderID == sh.RequiredGenderID
                                                   select g.GenderType).SingleOrDefault(),
                                 RequiredWorkType = (from w in db.GetTable<Work>()
                                                     where w.WorkID == sh.RequiredWorkID
                                                     select w.WorkType).SingleOrDefault(),
                                 AllowSmoking = sh.AllowSmoking,
                                 AllowAlcohol = sh.AllowAlcohol,
                                 HasPrivateKey = sh.HasPrivateKey,
                                 DateTimeCreated = DateTimeOffset.UtcNow
                             },
                             DateTimeCreated = ssh.DateTimeCreated
                         }).Take(5).ToList();

                    if (newerSavedShareHousings != null & newerSavedShareHousings.Count > 0)
                    {
                        foreach (var item in newerSavedShareHousings)
                        {
                            item.SavedShareHousing.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                                   join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                   where (a.HousingID == item.SavedShareHousing.Housing.ID)
                                                                      && (a.CreatorID == item.SavedShareHousing.Housing.Owner.UserID)
                                                                   orderby p.PhotoID
                                                                   select p.PhotoLink).ToList();
                        }
                    }
                }
                return newerSavedShareHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/sharehousing")]
        [HttpGet]
        [Authorize]
        public bool GetSavingStateOfCurrentShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where sh.ShareHousingID == shareHousingID
                                               select sh).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        var oldSavedShareHousing = (from ss in db.GetTable<SavedShareHousing>()
                                                    where (ss.ShareHousingID == currentShareHousing.ShareHousingID)
                                                       && (ss.CreatorID == currentUser.UserID)
                                                    select ss).SingleOrDefault();
                        if (oldSavedShareHousing != null)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("save/sharehousing")]
        [HttpPost]
        [Authorize]
        public SavedShareHousingModel SaveShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where sh.ShareHousingID == shareHousingID
                                               select sh).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                          where u.UserID == currentShareHousing.CreatorID
                                                          select u).SingleOrDefault();
                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                        if (currentShareHousingCreator != null && oldHousing != null)
                        {
                            var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == oldHousing.OwnerID
                                                   select u).SingleOrDefault();
                            var oldAddress = (from a in db.GetTable<Address>()
                                              join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                              join c in db.GetTable<City>() on a.CityID equals c.CityID
                                              where a.AddressID == oldHousing.AddressID
                                              select new { a, gl, c }).SingleOrDefault();
                            if (oldHousingOwner != null && oldAddress != null)
                            {
                                var oldSavedShareHousing = (from ss in db.GetTable<SavedShareHousing>()
                                                            where (ss.ShareHousingID == currentShareHousing.ShareHousingID)
                                                               && (ss.CreatorID == currentUser.UserID)
                                                            select ss).SingleOrDefault();
                                if (oldSavedShareHousing == null)
                                {
                                    ++currentShareHousing.NumOfSaved;

                                    SavedShareHousing newSavedShareHousing = new SavedShareHousing
                                    {
                                        ShareHousingID = currentShareHousing.ShareHousingID,
                                        CreatorID = currentUser.UserID,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    };
                                    db.SavedShareHousings.InsertOnSubmit(newSavedShareHousing);
                                    db.SubmitChanges();
                                    return new SavedShareHousingModel
                                    {
                                        SavedShareHousing = new ShareHousingModel
                                        {
                                            ID = currentShareHousing.ShareHousingID,
                                            Housing = new HousingModel
                                            {
                                                ID = oldHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == oldHousing.HousingID)
                                                                && (a.CreatorID == oldHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = oldHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = oldHousingOwner.UserID,
                                                    FirstName = oldHousingOwner.FirstName,
                                                    LastName = oldHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == oldHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = oldHousingOwner.DOB,
                                                    PhoneNumber = oldHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == oldHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = oldHousingOwner.NumOfNote,
                                                    DeviceTokens = oldHousingOwner.DeviceTokens
                                                },
                                                Price = oldHousing.Price,
                                                IsAvailable = oldHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == oldHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = oldHousing.DateTimeCreated,
                                                NumOfView = oldHousing.NumOfView,
                                                NumOfSaved = oldHousing.NumOfSaved,
                                                NumOfPeople = oldHousing.NumOfPeople,
                                                NumOfRoom = oldHousing.NumOfRoom,
                                                NumOfBed = oldHousing.NumOfBed,
                                                NumOfBath = oldHousing.NumOfBath,
                                                AllowPet = oldHousing.AllowPet,
                                                HasWifi = oldHousing.HasWifi,
                                                HasAC = oldHousing.HasAC,
                                                HasParking = oldHousing.HasParking,
                                                TimeRestriction = oldHousing.TimeRestriction,
                                                Area = oldHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = oldHousing.Description,
                                                NumOfComment = oldHousing.NumOfComment
                                            },
                                            Creator = new UserModel
                                            {
                                                UserID = currentShareHousing.CreatorID,
                                                FirstName = currentShareHousingCreator.FirstName,
                                                LastName = currentShareHousingCreator.LastName,
                                                Email = (from user in db.GetTable<ShareSpaceUser>()
                                                         join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where user.UserID == currentShareHousingCreator.UserID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = currentShareHousingCreator.DOB,
                                                PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                          join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                          where user.UserID == currentShareHousingCreator.UserID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = currentShareHousingCreator.NumOfNote,
                                                DeviceTokens = currentShareHousingCreator.DeviceTokens
                                            },
                                            IsAvailable = currentShareHousing.IsAvailable,
                                            PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                            Description = currentShareHousing.Description,
                                            NumOfView = currentShareHousing.NumOfView,
                                            NumOfSaved = currentShareHousing.NumOfSaved,
                                            RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                            RequiredGender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentShareHousing.RequiredGenderID
                                                              select g.GenderType).SingleOrDefault(),
                                            RequiredWorkType = (from w in db.GetTable<Work>()
                                                                where w.WorkID == currentShareHousing.RequiredWorkID
                                                                select w.WorkType).SingleOrDefault(),
                                            AllowSmoking = currentShareHousing.AllowSmoking,
                                            AllowAlcohol = currentShareHousing.AllowAlcohol,
                                            HasPrivateKey = currentShareHousing.HasPrivateKey,
                                            DateTimeCreated = DateTimeOffset.UtcNow
                                        },
                                        DateTimeCreated = newSavedShareHousing.DateTimeCreated
                                    };
                                }
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

        [Route("save/sharehousing")]
        [HttpDelete]
        [Authorize]
        public SavedShareHousingModel UnsaveShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where sh.ShareHousingID == shareHousingID
                                               select sh).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                          where u.UserID == currentShareHousing.CreatorID
                                                          select u).SingleOrDefault();
                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                        if (currentShareHousingCreator != null && oldHousing != null)
                        {
                            var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == oldHousing.OwnerID
                                                   select u).SingleOrDefault();
                            var oldAddress = (from a in db.GetTable<Address>()
                                              join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                              join c in db.GetTable<City>() on a.CityID equals c.CityID
                                              where a.AddressID == oldHousing.AddressID
                                              select new { a, gl, c }).SingleOrDefault();
                            if (oldHousingOwner != null && oldAddress != null)
                            {
                                var oldSavedShareHousing = (from ss in db.GetTable<SavedShareHousing>()
                                                            where (ss.ShareHousingID == currentShareHousing.ShareHousingID)
                                                               && (ss.CreatorID == currentUser.UserID)
                                                            select ss).SingleOrDefault();
                                if (oldSavedShareHousing != null)
                                {
                                    --currentShareHousing.NumOfSaved;
                                    db.SubmitChanges();

                                    SavedShareHousingModel ssh = new SavedShareHousingModel
                                    {
                                        SavedShareHousing = new ShareHousingModel
                                        {
                                            ID = currentShareHousing.ShareHousingID,
                                            Housing = new HousingModel
                                            {
                                                ID = oldHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == oldHousing.HousingID)
                                                                && (a.CreatorID == oldHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = oldHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = oldHousingOwner.UserID,
                                                    FirstName = oldHousingOwner.FirstName,
                                                    LastName = oldHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == oldHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = oldHousingOwner.DOB,
                                                    PhoneNumber = oldHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == oldHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = oldHousingOwner.NumOfNote,
                                                    DeviceTokens = oldHousingOwner.DeviceTokens
                                                },
                                                Price = oldHousing.Price,
                                                IsAvailable = oldHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == oldHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = oldHousing.DateTimeCreated,
                                                NumOfView = oldHousing.NumOfView,
                                                NumOfSaved = oldHousing.NumOfSaved,
                                                NumOfPeople = oldHousing.NumOfPeople,
                                                NumOfRoom = oldHousing.NumOfRoom,
                                                NumOfBed = oldHousing.NumOfBed,
                                                NumOfBath = oldHousing.NumOfBath,
                                                AllowPet = oldHousing.AllowPet,
                                                HasWifi = oldHousing.HasWifi,
                                                HasAC = oldHousing.HasAC,
                                                HasParking = oldHousing.HasParking,
                                                TimeRestriction = oldHousing.TimeRestriction,
                                                Area = oldHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = oldHousing.Description,
                                                NumOfComment = oldHousing.NumOfComment
                                            },
                                            Creator = new UserModel
                                            {
                                                UserID = currentShareHousing.CreatorID,
                                                FirstName = currentShareHousingCreator.FirstName,
                                                LastName = currentShareHousingCreator.LastName,
                                                Email = (from user in db.GetTable<ShareSpaceUser>()
                                                         join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where user.UserID == currentShareHousingCreator.UserID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = currentShareHousingCreator.DOB,
                                                PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                          join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                          where user.UserID == currentShareHousingCreator.UserID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = currentShareHousingCreator.NumOfNote,
                                                DeviceTokens = currentShareHousingCreator.DeviceTokens
                                            },
                                            IsAvailable = currentShareHousing.IsAvailable,
                                            PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                            Description = currentShareHousing.Description,
                                            NumOfView = currentShareHousing.NumOfView,
                                            NumOfSaved = currentShareHousing.NumOfSaved,
                                            RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                            RequiredGender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentShareHousing.RequiredGenderID
                                                              select g.GenderType).SingleOrDefault(),
                                            RequiredWorkType = (from w in db.GetTable<Work>()
                                                                where w.WorkID == currentShareHousing.RequiredWorkID
                                                                select w.WorkType).SingleOrDefault(),
                                            AllowSmoking = currentShareHousing.AllowSmoking,
                                            AllowAlcohol = currentShareHousing.AllowAlcohol,
                                            HasPrivateKey = currentShareHousing.HasPrivateKey,
                                            DateTimeCreated = DateTimeOffset.UtcNow
                                        },
                                        DateTimeCreated = oldSavedShareHousing.DateTimeCreated
                                    };
                                    db.SavedShareHousings.DeleteOnSubmit(oldSavedShareHousing);
                                    db.SubmitChanges();
                                    return ssh;
                                }
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

        [Route("appointment/housing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfHousingAppointments(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var housingAppointments = (from ha in db.GetTable<HousingAppointment>()
                                               where (ha.SenderID == currentUser.UserID)
                                                  || (ha.RecipientID == currentUser.UserID)
                                               select ha).ToList();
                    if (housingAppointments != null && housingAppointments.Count > 0)
                    {
                        return housingAppointments.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("appointment/housing")]
        [HttpGet]
        [Authorize]
        public List<HousingAppointmentModel> GetMoreOlderHousingAppointments(string currentBottomHousingAppointmentDateTimeCreated = null)
        {
            try
            {
                List<HousingAppointment> oldHousingAppointments = new List<HousingAppointment>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomHousingAppointmentDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();
                    if (currentBottomHousingAppointmentDateTimeCreated != null)
                    {
                        oldHousingAppointments = (from ha in db.GetTable<HousingAppointment>()
                                                  where ((ha.SenderID == currentUser.UserID) || (ha.RecipientID == currentUser.UserID))
                                                     && (ha.DateTimeCreated < parsedDate)
                                                  orderby ha.DateTimeCreated descending
                                                  select ha).Take(5).ToList();
                    }
                    else
                    {
                        var lastHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                      where (ha.SenderID == currentUser.UserID) || (ha.RecipientID == currentUser.UserID)
                                                      orderby ha.DateTimeCreated descending
                                                      select ha).Take(1).SingleOrDefault();
                        if (lastHousingAppointment != null)
                        {
                            oldHousingAppointments.Add(lastHousingAppointment);
                        }
                    }
                    
                    if (oldHousingAppointments != null && oldHousingAppointments.Count > 0)
                    {
                        List<HousingAppointmentModel> appointments = new List<HousingAppointmentModel>();
                        foreach (var appointment in oldHousingAppointments)
                        {
                            var currentHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == appointment.HousingID
                                                  select h).SingleOrDefault();
                            if (currentHousing != null)
                            {
                                var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == currentHousing.OwnerID
                                                           select u).SingleOrDefault();
                                var oldAddress = (from a in db.GetTable<Address>()
                                                  join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                  where a.AddressID == currentHousing.AddressID
                                                  select new { a, gl, c }).SingleOrDefault();
                                if (currentHousingOwner != null && oldAddress != null)
                                {
                                    var currentAppointmentSender = (from u in db.GetTable<ShareSpaceUser>()
                                                                    where u.UserID == appointment.SenderID
                                                                    select u).SingleOrDefault();
                                    if (currentAppointmentSender != null)
                                    {
                                        appointments.Add(new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = currentAppointmentSender.UserID,
                                                FirstName = currentAppointmentSender.FirstName,
                                                LastName = currentAppointmentSender.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == currentAppointmentSender.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = currentAppointmentSender.DOB,
                                                PhoneNumber = currentAppointmentSender.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == currentAppointmentSender.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = currentAppointmentSender.NumOfNote,
                                                DeviceTokens = currentAppointmentSender.DeviceTokens
                                            },
                                            RecipientID = appointment.RecipientID,
                                            AppointmentDateTime = appointment.AppointmentDateTime,
                                            DateTimeCreated = appointment.DateTimeCreated,
                                            Content = appointment.Content,
                                            IsOwnerConfirmed = appointment.IsOwnerConfirmed,
                                            IsUserConfirmed = appointment.IsUserConfirmed,
                                            NumOfRequests = appointment.NumOfRequests
                                        });
                                    }
                                }
                            }
                        }
                        if (appointments != null && appointments.Count > 0)
                        {
                            return appointments;
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

        //[Route("appointment/housing")]
        //[HttpGet]
        //[Authorize]
        //public List<HousingAppointmentModel> GetMoreNewerHousingAppointments(string currentTopHousingAppointmentDateTimeCreated)
        //{
        //    try
        //    {
        //        List<HousingAppointment> oldHousingAppointments = new List<HousingAppointment>();

        //        UserModel currentUser = base.GetCurrentUserInfo();

        //        DateTimeOffset parsedDate = base.ParseDateFromString(currentTopHousingAppointmentDateTimeCreated);

        //        if (currentUser != null)
        //        {
        //            DBShareSpaceDataContext db = new DBShareSpaceDataContext();
        //            if (currentTopHousingAppointmentDateTimeCreated != null)
        //            {
        //                oldHousingAppointments = (from ha in db.GetTable<HousingAppointment>()
        //                                          where ((ha.SenderID == currentUser.UserID) || (ha.RecipientID == currentUser.UserID))
        //                                             && (ha.DateTimeCreated > parsedDate)
        //                                          orderby ha.DateTimeCreated descending
        //                                          select ha).Take(5).ToList();
        //            }
        //            else
        //            {
        //                var lastHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
        //                                              where (ha.SenderID == currentUser.UserID) || (ha.RecipientID == currentUser.UserID)
        //                                              select ha).LastOrDefault();
        //                if (lastHousingAppointment != null)
        //                {
        //                    oldHousingAppointments.Add(lastHousingAppointment);
        //                }
        //            }

        //            if (oldHousingAppointments != null && oldHousingAppointments.Count > 0)
        //            {
        //                List<HousingAppointmentModel> appointments = new List<HousingAppointmentModel>();
        //                foreach (var appointment in oldHousingAppointments)
        //                {
        //                    var currentHousing = (from h in db.GetTable<Housing>()
        //                                          where h.HousingID == appointment.HousingID
        //                                          select h).SingleOrDefault();
        //                    if (currentHousing != null)
        //                    {
        //                        var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
        //                                                   where u.UserID == currentHousing.OwnerID
        //                                                   select u).SingleOrDefault();
        //                        var oldAddress = (from a in db.GetTable<Address>()
        //                                          join c in db.GetTable<City>() on a.CityID equals c.CityID
        //                                          where a.AddressID == currentHousing.AddressID
        //                                          select new { a, c }).SingleOrDefault();
        //                        if (currentHousingOwner != null && oldAddress != null)
        //                        {
        //                            appointments.Add(new HousingAppointmentModel
        //                            {
        //                                Housing = new HousingModel
        //                                {
        //                                    ID = currentHousing.HousingID,
        //                                    PhotoURLs = (from a in db.GetTable<Album>()
        //                                                 join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
        //                                                 where (a.HousingID == currentHousing.HousingID)
        //                                                    && (a.CreatorID == currentHousing.OwnerID)
        //                                                 select p.PhotoLink).ToList(),
        //                                    Title = currentHousing.Title,
        //                                    Owner = new UserModel
        //                                    {
        //                                        UserID = currentHousing.OwnerID,
        //                                        FirstName = currentHousingOwner.FirstName,
        //                                        LastName = currentHousingOwner.LastName,
        //                                        Email = (from e in db.GetTable<Email>()
        //                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
        //                                                 where e.EmailID == currentHousingOwner.EmailID
        //                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
        //                                        DOB = currentHousingOwner.DOB,
        //                                        PhoneNumber = currentHousingOwner.PhoneNumber,
        //                                        Gender = (from g in db.GetTable<Gender>()
        //                                                  where g.GenderID == currentHousingOwner.GenderID
        //                                                  select g.GenderType).SingleOrDefault(),
        //                                        NumOfNote = currentHousingOwner.NumOfNote,
        //                                        DeviceTokens = currentHousingOwner.DeviceTokens
        //                                    },
        //                                    Price = currentHousing.Price,
        //                                    IsAvailable = currentHousing.IsAvailable,
        //                                    HouseType = (from ht in db.GetTable<HouseType>()
        //                                                 where ht.HouseTypeID == currentHousing.HouseTypeID
        //                                                 select ht.HousingType).SingleOrDefault(),
        //                                    DateTimeCreated = currentHousing.DateTimeCreated,
        //                                    NumOfView = currentHousing.NumOfView,
        //                                    NumOfSaved = currentHousing.NumOfSaved,
        //                                    NumOfPeople = currentHousing.NumOfPeople,
        //                                    NumOfRoom = currentHousing.NumOfRoom,
        //                                    NumOfBed = currentHousing.NumOfBed,
        //                                    NumOfBath = currentHousing.NumOfBath,
        //                                    AllowPet = currentHousing.AllowPet,
        //                                    HasWifi = currentHousing.HasWifi,
        //                                    HasAC = currentHousing.HasAC,
        //                                    HasParking = currentHousing.HasParking,
        //                                    TimeRestriction = currentHousing.TimeRestriction,
        //                                    Area = currentHousing.Area,
        //                                    Latitude = oldAddress.a.Latitude,
        //                                    Longitude = oldAddress.a.Longitude,
        //                                    AddressHouseNumber = oldAddress.a.HouseNumber,
        //                                    AddressStreet = oldAddress.a.Street,
        //                                    AddressWard = oldAddress.a.Ward,
        //                                    AddressDistrict = oldAddress.a.District,
        //                                    AddressCity = oldAddress.c.CityName,
        //                                    Description = currentHousing.Description,
        //                                    NumOfComment = currentHousing.NumOfComment
        //                                },
        //                                SenderID = appointment.SenderID,
        //                                RecipientID = appointment.RecipientID,
        //                                AppointmentDateTime = appointment.AppointmentDateTime,
        //                                DateTimeCreated = appointment.DateTimeCreated,
        //                                Content = appointment.Content,
        //                                IsOwnerConfirmed = appointment.IsOwnerConfirmed,
        //                                IsUserConfirmed = appointment.IsUserConfirmed,
        //                                NumOfRequests = appointment.NumOfRequests
        //                            });
        //                        }
        //                    }
        //                }
        //                if (appointments.Count > 0)
        //                {
        //                    return appointments;
        //                }
        //            }
        //        }
        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw e;
        //    }
        //}

        [Route("appointment/housing")]
        [HttpGet]
        [Authorize]
        // Only User can get each Housing Appointment when open Housing Detail Activity.
        public HousingAppointmentModel GetHousingAppointment(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == housingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == currentHousing.OwnerID
                                                   select u).SingleOrDefault();
                        var oldAddress = (from a in db.GetTable<Address>()
                                          join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                          join c in db.GetTable<City>() on a.CityID equals c.CityID
                                          where a.AddressID == currentHousing.AddressID
                                          select new { a, gl, c }).SingleOrDefault();
                        if (currentHousingOwner != null && oldAddress != null)
                        {
                            var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                         where (ha.HousingID == currentHousing.HousingID)
                                                            && (ha.SenderID == currentUser.UserID)
                                                            && (ha.RecipientID == currentHousing.OwnerID)
                                                         select ha).SingleOrDefault();
                            if (oldHousingAppointment != null)
                            {
                                return new HousingAppointmentModel
                                {
                                    Housing = new HousingModel
                                    {
                                        ID = currentHousing.HousingID,
                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                     where (a.HousingID == currentHousing.HousingID)
                                                        && (a.CreatorID == currentHousing.OwnerID)
                                                     select p.PhotoLink).ToList(),
                                        Title = currentHousing.Title,
                                        Owner = new UserModel
                                        {
                                            UserID = currentHousing.OwnerID,
                                            FirstName = currentHousingOwner.FirstName,
                                            LastName = currentHousingOwner.LastName,
                                            Email = (from e in db.GetTable<Email>()
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where e.EmailID == currentHousingOwner.EmailID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentHousingOwner.DOB,
                                            PhoneNumber = currentHousingOwner.PhoneNumber,
                                            Gender = (from g in db.GetTable<Gender>()
                                                      where g.GenderID == currentHousingOwner.GenderID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentHousingOwner.NumOfNote,
                                            DeviceTokens = currentHousingOwner.DeviceTokens
                                        },
                                        Price = currentHousing.Price,
                                        IsAvailable = currentHousing.IsAvailable,
                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                     where ht.HouseTypeID == currentHousing.HouseTypeID
                                                     select ht.HousingType).SingleOrDefault(),
                                        DateTimeCreated = currentHousing.DateTimeCreated,
                                        NumOfView = currentHousing.NumOfView,
                                        NumOfSaved = currentHousing.NumOfSaved,
                                        NumOfPeople = currentHousing.NumOfPeople,
                                        NumOfRoom = currentHousing.NumOfRoom,
                                        NumOfBed = currentHousing.NumOfBed,
                                        NumOfBath = currentHousing.NumOfBath,
                                        AllowPet = currentHousing.AllowPet,
                                        HasWifi = currentHousing.HasWifi,
                                        HasAC = currentHousing.HasAC,
                                        HasParking = currentHousing.HasParking,
                                        TimeRestriction = currentHousing.TimeRestriction,
                                        Area = currentHousing.Area,
                                        Latitude = oldAddress.gl.Latitude,
                                        Longitude = oldAddress.gl.Longitude,
                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                        AddressStreet = oldAddress.a.Street,
                                        AddressWard = oldAddress.a.Ward,
                                        AddressDistrict = oldAddress.a.District,
                                        AddressCity = oldAddress.c.CityName,
                                        Description = currentHousing.Description,
                                        NumOfComment = currentHousing.NumOfComment
                                    },
                                    Sender = new UserModel
                                    {
                                        UserID = currentUser.UserID,
                                        FirstName = currentUser.FirstName,
                                        LastName = currentUser.LastName,
                                        Email = currentUser.Email,
                                        DOB = currentUser.DOB,
                                        PhoneNumber = currentUser.PhoneNumber,
                                        Gender = currentUser.Gender,
                                        NumOfNote = currentUser.NumOfNote,
                                        DeviceTokens = currentUser.DeviceTokens
                                    },
                                    RecipientID = oldHousingAppointment.RecipientID,
                                    AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                    DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                    Content = oldHousingAppointment.Content,
                                    IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                    IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                    NumOfRequests = oldHousingAppointment.NumOfRequests
                                };
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

        [Route("appointment/housing")]
        [HttpGet]
        [Authorize]
        // Only User can get each Housing Appointment when open Housing Detail Activity.
        public HousingAppointmentModel GetHousingAppointment(int housingID, int userID, int ownerID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == housingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == currentHousing.OwnerID
                                                   select u).SingleOrDefault();
                        var oldAddress = (from a in db.GetTable<Address>()
                                          join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                          join c in db.GetTable<City>() on a.CityID equals c.CityID
                                          where a.AddressID == currentHousing.AddressID
                                          select new { a, gl, c }).SingleOrDefault();
                        if (currentHousingOwner != null && oldAddress != null)
                        {
                            var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                         where (ha.HousingID == currentHousing.HousingID)
                                                            && (ha.SenderID == userID)
                                                            && (ha.RecipientID == ownerID)
                                                         select ha).SingleOrDefault();
                            if (oldHousingAppointment != null)
                            {
                                return new HousingAppointmentModel
                                {
                                    Housing = new HousingModel
                                    {
                                        ID = currentHousing.HousingID,
                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                     where (a.HousingID == currentHousing.HousingID)
                                                        && (a.CreatorID == currentHousing.OwnerID)
                                                     select p.PhotoLink).ToList(),
                                        Title = currentHousing.Title,
                                        Owner = new UserModel
                                        {
                                            UserID = currentHousing.OwnerID,
                                            FirstName = currentHousingOwner.FirstName,
                                            LastName = currentHousingOwner.LastName,
                                            Email = (from e in db.GetTable<Email>()
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where e.EmailID == currentHousingOwner.EmailID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentHousingOwner.DOB,
                                            PhoneNumber = currentHousingOwner.PhoneNumber,
                                            Gender = (from g in db.GetTable<Gender>()
                                                      where g.GenderID == currentHousingOwner.GenderID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentHousingOwner.NumOfNote,
                                            DeviceTokens = currentHousingOwner.DeviceTokens
                                        },
                                        Price = currentHousing.Price,
                                        IsAvailable = currentHousing.IsAvailable,
                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                     where ht.HouseTypeID == currentHousing.HouseTypeID
                                                     select ht.HousingType).SingleOrDefault(),
                                        DateTimeCreated = currentHousing.DateTimeCreated,
                                        NumOfView = currentHousing.NumOfView,
                                        NumOfSaved = currentHousing.NumOfSaved,
                                        NumOfPeople = currentHousing.NumOfPeople,
                                        NumOfRoom = currentHousing.NumOfRoom,
                                        NumOfBed = currentHousing.NumOfBed,
                                        NumOfBath = currentHousing.NumOfBath,
                                        AllowPet = currentHousing.AllowPet,
                                        HasWifi = currentHousing.HasWifi,
                                        HasAC = currentHousing.HasAC,
                                        HasParking = currentHousing.HasParking,
                                        TimeRestriction = currentHousing.TimeRestriction,
                                        Area = currentHousing.Area,
                                        Latitude = oldAddress.gl.Latitude,
                                        Longitude = oldAddress.gl.Longitude,
                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                        AddressStreet = oldAddress.a.Street,
                                        AddressWard = oldAddress.a.Ward,
                                        AddressDistrict = oldAddress.a.District,
                                        AddressCity = oldAddress.c.CityName,
                                        Description = currentHousing.Description,
                                        NumOfComment = currentHousing.NumOfComment
                                    },
                                    Sender = new UserModel
                                    {
                                        UserID = currentUser.UserID,
                                        FirstName = currentUser.FirstName,
                                        LastName = currentUser.LastName,
                                        Email = currentUser.Email,
                                        DOB = currentUser.DOB,
                                        PhoneNumber = currentUser.PhoneNumber,
                                        Gender = currentUser.Gender,
                                        NumOfNote = currentUser.NumOfNote,
                                        DeviceTokens = currentUser.DeviceTokens
                                    },
                                    RecipientID = oldHousingAppointment.RecipientID,
                                    AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                    DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                    Content = oldHousingAppointment.Content,
                                    IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                    IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                    NumOfRequests = oldHousingAppointment.NumOfRequests
                                };
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

        [Route("appointment/housing")]
        [HttpPost]
        [Authorize]
        // Current Sender is who sent this Create New appointment request. Recipient is who receive this request (Housing Owner in this case).
        public HousingAppointmentModel SetNewHousingAppointment(int housingID, int recipientID, string appointmentDateTime, string content)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    DateTimeOffset parsedDate = base.ParseDateFromString(appointmentDateTime);

                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (recipient != null)
                    {
                        // There is no case of Housing Owner Send New Appointment Request to User.
                        var currentHousing = (from h in db.GetTable<Housing>()
                                              where (h.HousingID == housingID)
                                                 && (h.OwnerID == recipient.UserID)
                                              select h).SingleOrDefault();
                        if (currentHousing != null)
                        {
                            var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                         where (ha.HousingID == currentHousing.HousingID)
                                                            && (ha.SenderID == currentSender.UserID)
                                                            && (ha.RecipientID == currentHousing.OwnerID)
                                                         select ha).SingleOrDefault();
                            if (oldHousingAppointment != null)
                            {
                                var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == currentHousing.OwnerID
                                                           select u).SingleOrDefault();
                                var oldAddress = (from a in db.GetTable<Address>()
                                                  join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                  where a.AddressID == currentHousing.AddressID
                                                  select new { a, gl, c }).SingleOrDefault();
                                if (currentHousingOwner != null && oldAddress != null)
                                {
                                    // TODO: Edit to also cover update appointment condition.
                                    oldHousingAppointment.AppointmentDateTime = parsedDate;
                                    db.SubmitChanges();

                                    return new HousingAppointmentModel
                                    {
                                        Housing = new HousingModel
                                        {
                                            ID = currentHousing.HousingID,
                                            PhotoURLs = (from a in db.GetTable<Album>()
                                                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                         where (a.HousingID == currentHousing.HousingID)
                                                            && (a.CreatorID == currentHousing.OwnerID)
                                                         select p.PhotoLink).ToList(),
                                            Title = currentHousing.Title,
                                            Owner = new UserModel
                                            {
                                                UserID = currentHousing.OwnerID,
                                                FirstName = currentHousingOwner.FirstName,
                                                LastName = currentHousingOwner.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == currentHousingOwner.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = currentHousingOwner.DOB,
                                                PhoneNumber = currentHousingOwner.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == currentHousingOwner.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = currentHousingOwner.NumOfNote,
                                                DeviceTokens = currentHousingOwner.DeviceTokens
                                            },
                                            Price = currentHousing.Price,
                                            IsAvailable = currentHousing.IsAvailable,
                                            HouseType = (from ht in db.GetTable<HouseType>()
                                                         where ht.HouseTypeID == currentHousing.HouseTypeID
                                                         select ht.HousingType).SingleOrDefault(),
                                            DateTimeCreated = currentHousing.DateTimeCreated,
                                            NumOfView = currentHousing.NumOfView,
                                            NumOfSaved = currentHousing.NumOfSaved,
                                            NumOfPeople = currentHousing.NumOfPeople,
                                            NumOfRoom = currentHousing.NumOfRoom,
                                            NumOfBed = currentHousing.NumOfBed,
                                            NumOfBath = currentHousing.NumOfBath,
                                            AllowPet = currentHousing.AllowPet,
                                            HasWifi = currentHousing.HasWifi,
                                            HasAC = currentHousing.HasAC,
                                            HasParking = currentHousing.HasParking,
                                            TimeRestriction = currentHousing.TimeRestriction,
                                            Area = currentHousing.Area,
                                            Latitude = oldAddress.gl.Latitude,
                                            Longitude = oldAddress.gl.Longitude,
                                            AddressHouseNumber = oldAddress.a.HouseNumber,
                                            AddressStreet = oldAddress.a.Street,
                                            AddressWard = oldAddress.a.Ward,
                                            AddressDistrict = oldAddress.a.District,
                                            AddressCity = oldAddress.c.CityName,
                                            Description = currentHousing.Description,
                                            NumOfComment = currentHousing.NumOfComment
                                        },
                                        Sender = new UserModel
                                        {
                                            UserID = currentSender.UserID,
                                            FirstName = currentSender.FirstName,
                                            LastName = currentSender.LastName,
                                            Email = currentSender.Email,
                                            DOB = currentSender.DOB,
                                            PhoneNumber = currentSender.PhoneNumber,
                                            Gender = currentSender.Gender,
                                            NumOfNote = currentSender.NumOfNote,
                                            DeviceTokens = currentSender.DeviceTokens
                                        },
                                        RecipientID = oldHousingAppointment.RecipientID,
                                        AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                        DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                        Content = oldHousingAppointment.Content,
                                        IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                        IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                        NumOfRequests = oldHousingAppointment.NumOfRequests
                                    };
                                }                                
                            }
                            else
                            {
                                var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == currentHousing.OwnerID
                                                           select u).SingleOrDefault();
                                var oldAddress = (from a in db.GetTable<Address>()
                                                  join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                  where a.AddressID == currentHousing.AddressID
                                                  select new { a, gl, c }).SingleOrDefault();
                                if (currentHousingOwner != null && oldAddress != null)
                                {
                                    HousingAppointment newHousingAppointment = new HousingAppointment
                                    {
                                        HousingID = currentHousing.HousingID,
                                        SenderID = currentSender.UserID,
                                        RecipientID = currentHousing.OwnerID,
                                        AppointmentDateTime = parsedDate,
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        Content = content,
                                        IsOwnerConfirmed = false,
                                        IsUserConfirmed = true,
                                        NumOfRequests = 1
                                    };
                                    db.HousingAppointments.InsertOnSubmit(newHousingAppointment);
                                    db.SubmitChanges();

                                    return new HousingAppointmentModel
                                    {
                                        Housing = new HousingModel
                                        {
                                            ID = currentHousing.HousingID,
                                            PhotoURLs = (from a in db.GetTable<Album>()
                                                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                         where (a.HousingID == currentHousing.HousingID)
                                                            && (a.CreatorID == currentHousing.OwnerID)
                                                         select p.PhotoLink).ToList(),
                                            Title = currentHousing.Title,
                                            Owner = new UserModel
                                            {
                                                UserID = currentHousing.OwnerID,
                                                FirstName = currentHousingOwner.FirstName,
                                                LastName = currentHousingOwner.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == currentHousingOwner.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = currentHousingOwner.DOB,
                                                PhoneNumber = currentHousingOwner.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == currentHousingOwner.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = currentHousingOwner.NumOfNote,
                                                DeviceTokens = currentHousingOwner.DeviceTokens
                                            },
                                            Price = currentHousing.Price,
                                            IsAvailable = currentHousing.IsAvailable,
                                            HouseType = (from ht in db.GetTable<HouseType>()
                                                         where ht.HouseTypeID == currentHousing.HouseTypeID
                                                         select ht.HousingType).SingleOrDefault(),
                                            DateTimeCreated = currentHousing.DateTimeCreated,
                                            NumOfView = currentHousing.NumOfView,
                                            NumOfSaved = currentHousing.NumOfSaved,
                                            NumOfPeople = currentHousing.NumOfPeople,
                                            NumOfRoom = currentHousing.NumOfRoom,
                                            NumOfBed = currentHousing.NumOfBed,
                                            NumOfBath = currentHousing.NumOfBath,
                                            AllowPet = currentHousing.AllowPet,
                                            HasWifi = currentHousing.HasWifi,
                                            HasAC = currentHousing.HasAC,
                                            HasParking = currentHousing.HasParking,
                                            TimeRestriction = currentHousing.TimeRestriction,
                                            Area = currentHousing.Area,
                                            Latitude = oldAddress.gl.Latitude,
                                            Longitude = oldAddress.gl.Longitude,
                                            AddressHouseNumber = oldAddress.a.HouseNumber,
                                            AddressStreet = oldAddress.a.Street,
                                            AddressWard = oldAddress.a.Ward,
                                            AddressDistrict = oldAddress.a.District,
                                            AddressCity = oldAddress.c.CityName,
                                            Description = currentHousing.Description,
                                            NumOfComment = currentHousing.NumOfComment
                                        },
                                        Sender = new UserModel
                                        {
                                            UserID = currentSender.UserID,
                                            FirstName = currentSender.FirstName,
                                            LastName = currentSender.LastName,
                                            Email = currentSender.Email,
                                            DOB = currentSender.DOB,
                                            PhoneNumber = currentSender.PhoneNumber,
                                            Gender = currentSender.Gender,
                                            NumOfNote = currentSender.NumOfNote,
                                            DeviceTokens = currentSender.DeviceTokens
                                        },
                                        RecipientID = newHousingAppointment.RecipientID,
                                        AppointmentDateTime = newHousingAppointment.AppointmentDateTime,
                                        DateTimeCreated = newHousingAppointment.DateTimeCreated,
                                        Content = newHousingAppointment.Content,
                                        IsOwnerConfirmed = newHousingAppointment.IsOwnerConfirmed,
                                        IsUserConfirmed = newHousingAppointment.IsUserConfirmed,
                                        NumOfRequests = newHousingAppointment.NumOfRequests
                                    };
                                }
                            }
                            //var oldAddress = (from a in db.GetTable<Address>()
                            //                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                            //                  where a.AddressID == currentHousing.AddressID
                            //                  select new { a, c }).SingleOrDefault();
                            //if (oldAddress != null)
                            //{
                            //    return new AppointmentModel
                            //    {
                            //        Housing = new HousingModel
                            //        {
                            //            ID = currentHousing.HousingID,
                            //            PhotoURLs = (from a in db.GetTable<Album>()
                            //                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                            //                         where (a.HousingID == currentHousing.HousingID)
                            //                            && (a.CreatorID == currentHousing.OwnerID)
                            //                         select p.PhotoLink).ToList(),
                            //            Title = currentHousing.Title,
                            //            Owner = new UserModel
                            //            {
                            //                UserID = recipient.UserID,
                            //                FirstName = recipient.FirstName,
                            //                LastName = recipient.LastName,
                            //                Email = (from e in db.GetTable<Email>()
                            //                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                            //                         where e.EmailID == recipient.EmailID
                            //                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                            //                DOB = recipient.DOB,
                            //                PhoneNumber = recipient.PhoneNumber,
                            //                Gender = (from g in db.GetTable<Gender>()
                            //                          where g.GenderID == recipient.GenderID
                            //                          select g.GenderType).SingleOrDefault(),
                            //                NumOfNote = recipient.NumOfNote
                            //            },
                            //            Price = currentHousing.Price,
                            //            IsAvailable = currentHousing.IsAvailable,
                            //            HouseType = (from ht in db.GetTable<HouseType>()
                            //                         where ht.HouseTypeID == currentHousing.HouseTypeID
                            //                         select ht.HousingType).SingleOrDefault(),
                            //            DateTimeCreated = currentHousing.DateTimeCreated,
                            //            NumOfView = currentHousing.NumOfView,
                            //            NumOfSaved = currentHousing.NumOfSaved,
                            //            NumOfPeople = currentHousing.NumOfPeople,
                            //            NumOfRoom = currentHousing.NumOfRoom,
                            //            NumOfBed = currentHousing.NumOfBed,
                            //            NumOfBath = currentHousing.NumOfBath,
                            //            AllowPet = currentHousing.AllowPet,
                            //            HasWifi = currentHousing.HasWifi,
                            //            HasAC = currentHousing.HasAC,
                            //            HasParking = currentHousing.HasParking,
                            //            TimeRestriction = currentHousing.TimeRestriction,
                            //            Area = currentHousing.Area,
                            //            Latitude = oldAddress.a.Latitude,
                            //            Longitude = oldAddress.a.Longitude,
                            //            AddressHouseNumber = oldAddress.a.HouseNumber,
                            //            AddressStreet = oldAddress.a.Street,
                            //            AddressWard = oldAddress.a.Ward,
                            //            AddressDistrict = oldAddress.a.District,
                            //            AddressCity = oldAddress.c.CityName,
                            //            Description = currentHousing.Description,
                            //            NumOfComment = currentHousing.NumOfComment
                            //        },
                            //        SenderID = currentSender.UserID,
                            //        RecipientID = currentHousing.OwnerID,
                            //        AppointmentDateTime = appointmentDateTime
                            //    };
                            //}
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

        [Route("appointment/housing")]
        [HttpPut]
        [Authorize]
        // Current Sender is who sent this Update request. Recipient is who receive this request.
        public HousingAppointmentModel UpdateHousingAppointment(int housingID, int recipientID, string appointmentDateTime, string content)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    DateTimeOffset parsedDate = base.ParseDateFromString(appointmentDateTime);

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Housing Owner and User can send Update request.
                        var currentHousing = (from h in db.GetTable<Housing>()
                                              where (h.HousingID == housingID)
                                              select h).SingleOrDefault();
                        if (currentHousing != null)
                        {
                            // Housing Owner is sending this Update request.
                            if (sender.UserID == currentHousing.OwnerID)
                            {
                                var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                             where (ha.HousingID == currentHousing.HousingID)
                                                                && (ha.SenderID == recipient.UserID)    // => User is recipient.
                                                                && (ha.RecipientID == currentHousing.OwnerID)
                                                             select ha).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        oldHousingAppointment.AppointmentDateTime = parsedDate;
                                        oldHousingAppointment.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousingAppointment.Content = content;
                                        oldHousingAppointment.IsOwnerConfirmed = true;
                                        oldHousingAppointment.IsUserConfirmed = false;
                                        oldHousingAppointment.NumOfRequests = ++oldHousingAppointment.NumOfRequests;
                                        db.SubmitChanges();

                                        return new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = recipient.UserID,
                                                FirstName = recipient.FirstName,
                                                LastName = recipient.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == recipient.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = recipient.DOB,
                                                PhoneNumber = recipient.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == recipient.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = recipient.NumOfNote,
                                                DeviceTokens = recipient.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                    }                                    
                                }
                            }
                            else if (recipient.UserID == currentHousing.OwnerID)    // User is sending this Update request.
                            {
                                var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                             where (ha.HousingID == currentHousing.HousingID)
                                                                && (ha.SenderID == sender.UserID)
                                                                && (ha.RecipientID == currentHousing.OwnerID)    // => Housing Owner is recipient.
                                                             select ha).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        oldHousingAppointment.AppointmentDateTime = parsedDate;
                                        oldHousingAppointment.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousingAppointment.Content = content;
                                        oldHousingAppointment.IsOwnerConfirmed = false;
                                        oldHousingAppointment.IsUserConfirmed = true;
                                        oldHousingAppointment.NumOfRequests = ++oldHousingAppointment.NumOfRequests;
                                        db.SubmitChanges();

                                        return new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = sender.UserID,
                                                FirstName = sender.FirstName,
                                                LastName = sender.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == sender.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = sender.DOB,
                                                PhoneNumber = sender.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == sender.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = sender.NumOfNote,
                                                DeviceTokens = sender.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                    }
                                }
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

        [Route("appointment/housing")]
        [HttpDelete]
        [Authorize]
        // Current Sender is who sent this Delete request. Recipient is who receive this request.
        public HousingAppointmentModel DeleteHousingAppointment(int housingID, int recipientID)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Housing Owner and User can send Delete request.
                        var currentHousing = (from h in db.GetTable<Housing>()
                                              where (h.HousingID == housingID)
                                              select h).SingleOrDefault();
                        if (currentHousing != null)
                        {
                            // Housing Owner is sending this Delete request.
                            if (sender.UserID == currentHousing.OwnerID)
                            {
                                var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                             where (ha.HousingID == currentHousing.HousingID)
                                                                && (ha.SenderID == recipient.UserID)    // => User is recipient.
                                                                && (ha.RecipientID == currentHousing.OwnerID)
                                                             select ha).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        HousingAppointmentModel ha = new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = recipient.UserID,
                                                FirstName = recipient.FirstName,
                                                LastName = recipient.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == recipient.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = recipient.DOB,
                                                PhoneNumber = recipient.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == recipient.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = recipient.NumOfNote,
                                                DeviceTokens = recipient.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                        db.HousingAppointments.DeleteOnSubmit(oldHousingAppointment);
                                        db.SubmitChanges();
                                        return ha;
                                    }                                    
                                }
                            }
                            else if (recipient.UserID == currentHousing.OwnerID)    // User is sending this Delete request.
                            {
                                var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                             where (ha.HousingID == currentHousing.HousingID)
                                                                && (ha.SenderID == sender.UserID)
                                                                && (ha.RecipientID == currentHousing.OwnerID)    // => Housing Owner is recipient.
                                                             select ha).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        HousingAppointmentModel ha = new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = sender.UserID,
                                                FirstName = sender.FirstName,
                                                LastName = sender.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == sender.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = sender.DOB,
                                                PhoneNumber = sender.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == sender.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = sender.NumOfNote,
                                                DeviceTokens = sender.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                        db.HousingAppointments.DeleteOnSubmit(oldHousingAppointment);
                                        db.SubmitChanges();
                                        return ha;
                                    }
                                }
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

        [Route("appointment/housing")]
        [HttpPut]
        [Authorize]
        // Current Sender is who sent this Accept request. Recipient is who receive this request.
        public HousingAppointmentModel AcceptHousingAppointment(int housingID, int recipientID)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Housing Owner and User can send Accept request.
                        // Only User has right to create an (send a new request of) appointment.
                        // => Only from 2nd update request (Housing Owner send update request to User),
                        //    User has right to accept the appointment.

                        var currentHousing = (from h in db.GetTable<Housing>()
                                              where (h.HousingID == housingID)
                                              select h).SingleOrDefault();
                        if (currentHousing != null)
                        {
                            // Housing Owner is sending this Accept request.
                            if (sender.UserID == currentHousing.OwnerID)
                            {
                                var oldHousingAppointment = (from ha in db.GetTable<HousingAppointment>()
                                                             where (ha.HousingID == currentHousing.HousingID)
                                                                && (ha.SenderID == recipient.UserID)    // => User is recipient.
                                                                && (ha.RecipientID == currentHousing.OwnerID)
                                                             select ha).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        oldHousingAppointment.IsOwnerConfirmed = true;
                                        oldHousingAppointment.IsUserConfirmed = true;
                                        oldHousingAppointment.NumOfRequests = ++oldHousingAppointment.NumOfRequests;
                                        db.SubmitChanges();

                                        return new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = recipient.UserID,
                                                FirstName = recipient.FirstName,
                                                LastName = recipient.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == recipient.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = recipient.DOB,
                                                PhoneNumber = recipient.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == recipient.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = recipient.NumOfNote,
                                                DeviceTokens = recipient.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                    }
                                }
                            }
                            else if (recipient.UserID == currentHousing.OwnerID)    // User is sending this Accept request.
                            {
                                var oldHousingAppointment = (from a in db.GetTable<HousingAppointment>()
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.SenderID == sender.UserID)
                                                                && (a.RecipientID == currentHousing.OwnerID)    // => Housing Owner is recipient.
                                                             select a).SingleOrDefault();
                                if (oldHousingAppointment != null)
                                {
                                    var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                               where u.UserID == currentHousing.OwnerID
                                                               select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == currentHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (currentHousingOwner != null && oldAddress != null)
                                    {
                                        oldHousingAppointment.IsOwnerConfirmed = true;
                                        oldHousingAppointment.IsUserConfirmed = true;
                                        oldHousingAppointment.NumOfRequests = ++oldHousingAppointment.NumOfRequests;
                                        db.SubmitChanges();

                                        return new HousingAppointmentModel
                                        {
                                            Housing = new HousingModel
                                            {
                                                ID = currentHousing.HousingID,
                                                PhotoURLs = (from a in db.GetTable<Album>()
                                                             join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                             where (a.HousingID == currentHousing.HousingID)
                                                                && (a.CreatorID == currentHousing.OwnerID)
                                                             select p.PhotoLink).ToList(),
                                                Title = currentHousing.Title,
                                                Owner = new UserModel
                                                {
                                                    UserID = currentHousing.OwnerID,
                                                    FirstName = currentHousingOwner.FirstName,
                                                    LastName = currentHousingOwner.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentHousingOwner.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentHousingOwner.DOB,
                                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentHousingOwner.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentHousingOwner.NumOfNote,
                                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                                },
                                                Price = currentHousing.Price,
                                                IsAvailable = currentHousing.IsAvailable,
                                                HouseType = (from ht in db.GetTable<HouseType>()
                                                             where ht.HouseTypeID == currentHousing.HouseTypeID
                                                             select ht.HousingType).SingleOrDefault(),
                                                DateTimeCreated = currentHousing.DateTimeCreated,
                                                NumOfView = currentHousing.NumOfView,
                                                NumOfSaved = currentHousing.NumOfSaved,
                                                NumOfPeople = currentHousing.NumOfPeople,
                                                NumOfRoom = currentHousing.NumOfRoom,
                                                NumOfBed = currentHousing.NumOfBed,
                                                NumOfBath = currentHousing.NumOfBath,
                                                AllowPet = currentHousing.AllowPet,
                                                HasWifi = currentHousing.HasWifi,
                                                HasAC = currentHousing.HasAC,
                                                HasParking = currentHousing.HasParking,
                                                TimeRestriction = currentHousing.TimeRestriction,
                                                Area = currentHousing.Area,
                                                Latitude = oldAddress.gl.Latitude,
                                                Longitude = oldAddress.gl.Longitude,
                                                AddressHouseNumber = oldAddress.a.HouseNumber,
                                                AddressStreet = oldAddress.a.Street,
                                                AddressWard = oldAddress.a.Ward,
                                                AddressDistrict = oldAddress.a.District,
                                                AddressCity = oldAddress.c.CityName,
                                                Description = currentHousing.Description,
                                                NumOfComment = currentHousing.NumOfComment
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = sender.UserID,
                                                FirstName = sender.FirstName,
                                                LastName = sender.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == sender.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = sender.DOB,
                                                PhoneNumber = sender.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == sender.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = sender.NumOfNote,
                                                DeviceTokens = sender.DeviceTokens
                                            },
                                            RecipientID = oldHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldHousingAppointment.DateTimeCreated,
                                            Content = oldHousingAppointment.Content,
                                            IsOwnerConfirmed = oldHousingAppointment.IsOwnerConfirmed,
                                            IsUserConfirmed = oldHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldHousingAppointment.NumOfRequests
                                        };
                                    }
                                }
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

        [Route("appointment/sharehousing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfShareHousingAppointments(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var shareHousingAppointments = (from sa in db.GetTable<ShareHousingAppointment>()
                                                    where (sa.SenderID == currentUser.UserID)
                                                       || (sa.RecipientID == currentUser.UserID)
                                                    select sa).ToList();
                    if (shareHousingAppointments != null && shareHousingAppointments.Count > 0)
                    {
                        return shareHousingAppointments.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("appointment/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<ShareHousingAppointmentModel> GetMoreOlderShareHousingAppointments(string currentBottomShareHousingAppointmentDateTimeCreated = null)
        {
            try
            {
                List<ShareHousingAppointment> oldShareHousingAppointments = new List<ShareHousingAppointment>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomShareHousingAppointmentDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (currentBottomShareHousingAppointmentDateTimeCreated != null)
                    {
                        oldShareHousingAppointments = (from sa in db.GetTable<ShareHousingAppointment>()
                                                       where ((sa.SenderID == currentUser.UserID) || (sa.RecipientID == currentUser.UserID))
                                                          && (sa.DateTimeCreated < parsedDate)
                                                       orderby sa.DateTimeCreated descending
                                                       select sa).Take(5).ToList();
                    }
                    else
                    {
                        var lastShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                           where (sa.SenderID == currentUser.UserID) || (sa.RecipientID == currentUser.UserID)
                                                           orderby sa.DateTimeCreated descending
                                                           select sa).Take(1).SingleOrDefault();
                        if (lastShareHousingAppointment != null)
                        {
                            oldShareHousingAppointments.Add(lastShareHousingAppointment);
                        }
                    }

                    if (oldShareHousingAppointments != null && oldShareHousingAppointments.Count > 0)
                    {
                        List<ShareHousingAppointmentModel> appointments = new List<ShareHousingAppointmentModel>();
                        foreach (var appointment in oldShareHousingAppointments)
                        {
                            var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                                       where sh.ShareHousingID == appointment.ShareHousingID
                                                       select sh).SingleOrDefault();
                            if (currentShareHousing != null)
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var currentAppointmentSender = (from u in db.GetTable<ShareSpaceUser>()
                                                                        where u.UserID == appointment.SenderID
                                                                        select u).SingleOrDefault();
                                        if (currentAppointmentSender != null)
                                        {
                                            appointments.Add(new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = currentAppointmentSender.UserID,
                                                    FirstName = currentAppointmentSender.FirstName,
                                                    LastName = currentAppointmentSender.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == currentAppointmentSender.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentAppointmentSender.DOB,
                                                    PhoneNumber = currentAppointmentSender.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == currentAppointmentSender.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentAppointmentSender.NumOfNote,
                                                    DeviceTokens = currentAppointmentSender.DeviceTokens
                                                },
                                                RecipientID = appointment.RecipientID,
                                                AppointmentDateTime = appointment.AppointmentDateTime,
                                                DateTimeCreated = appointment.DateTimeCreated,
                                                Content = appointment.Content,
                                                IsOwnerConfirmed = appointment.IsCreatorConfirmed,
                                                IsUserConfirmed = appointment.IsUserConfirmed,
                                                NumOfRequests = appointment.NumOfRequests
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        if (appointments != null && appointments.Count > 0)
                        {
                            return appointments;
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

        //[Route("appointment/sharehousing")]
        //[HttpGet]
        //[Authorize]
        //public List<ShareHousingAppointmentModel> GetMoreNewerShareHousingAppointments(string currentTopShareHousingAppointmentDateTimeCreated)
        //{
        //    try
        //    {
        //        List<ShareHousingAppointment> oldShareHousingAppointments = new List<ShareHousingAppointment>();

        //        UserModel currentUser = base.GetCurrentUserInfo();

        //        DateTimeOffset parsedDate = base.ParseDateFromString(currentTopShareHousingAppointmentDateTimeCreated);

        //        if (currentUser != null)
        //        {
        //            DBShareSpaceDataContext db = new DBShareSpaceDataContext();

        //            if (currentTopShareHousingAppointmentDateTimeCreated != null)
        //            {
        //                oldShareHousingAppointments = (from sa in db.GetTable<ShareHousingAppointment>()
        //                                               where ((sa.SenderID == currentUser.UserID) || (sa.RecipientID == currentUser.UserID))
        //                                                  && (sa.DateTimeCreated > parsedDate)
        //                                               orderby sa.DateTimeCreated descending
        //                                               select sa).Take(5).ToList();
        //            }
        //            else
        //            {
        //                var lastShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
        //                                                   where (sa.SenderID == currentUser.UserID) || (sa.RecipientID == currentUser.UserID)
        //                                                   select sa).LastOrDefault();
        //                if (lastShareHousingAppointment != null)
        //                {
        //                    oldShareHousingAppointments.Add(lastShareHousingAppointment);
        //                }
        //            }

        //            if (oldShareHousingAppointments != null && oldShareHousingAppointments.Count > 0)
        //            {
        //                List<ShareHousingAppointmentModel> appointments = new List<ShareHousingAppointmentModel>();
        //                foreach (var appointment in oldShareHousingAppointments)
        //                {
        //                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
        //                                               where sh.ShareHousingID == appointment.ShareHousingID
        //                                               select sh).SingleOrDefault();
        //                    if (currentShareHousing != null)
        //                    {
        //                        var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
        //                                                          where u.UserID == currentShareHousing.CreatorID
        //                                                          select u).SingleOrDefault();
        //                        var oldHousing = (from h in db.GetTable<Housing>()
        //                                          where h.HousingID == currentShareHousing.HousingID
        //                                          select h).SingleOrDefault();
        //                        if (currentShareHousingCreator != null && oldHousing != null)
        //                        {
        //                            var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
        //                                                   where u.UserID == oldHousing.OwnerID
        //                                                   select u).SingleOrDefault();
        //                            var oldAddress = (from a in db.GetTable<Address>()
        //                                              join c in db.GetTable<City>() on a.CityID equals c.CityID
        //                                              where a.AddressID == oldHousing.AddressID
        //                                              select new { a, c }).SingleOrDefault();
        //                            if (oldHousingOwner != null && oldAddress != null)
        //                            {
        //                                appointments.Add(new ShareHousingAppointmentModel
        //                                {
        //                                    ShareHousing = new ShareHousingModel
        //                                    {
        //                                        ID = currentShareHousing.ShareHousingID,
        //                                        Housing = new HousingModel
        //                                        {
        //                                            ID = oldHousing.HousingID,
        //                                            PhotoURLs = (from a in db.GetTable<Album>()
        //                                                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
        //                                                         where (a.HousingID == oldHousing.HousingID)
        //                                                            && (a.CreatorID == oldHousing.OwnerID)
        //                                                         select p.PhotoLink).ToList(),
        //                                            Title = oldHousing.Title,
        //                                            Owner = new UserModel
        //                                            {
        //                                                UserID = oldHousingOwner.UserID,
        //                                                FirstName = oldHousingOwner.FirstName,
        //                                                LastName = oldHousingOwner.LastName,
        //                                                Email = (from e in db.GetTable<Email>()
        //                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
        //                                                         where e.EmailID == oldHousingOwner.EmailID
        //                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
        //                                                DOB = oldHousingOwner.DOB,
        //                                                PhoneNumber = oldHousingOwner.PhoneNumber,
        //                                                Gender = (from g in db.GetTable<Gender>()
        //                                                          where g.GenderID == oldHousingOwner.GenderID
        //                                                          select g.GenderType).SingleOrDefault(),
        //                                                NumOfNote = oldHousingOwner.NumOfNote,
        //                                                DeviceTokens = oldHousingOwner.DeviceTokens
        //                                            },
        //                                            Price = oldHousing.Price,
        //                                            IsAvailable = oldHousing.IsAvailable,
        //                                            HouseType = (from ht in db.GetTable<HouseType>()
        //                                                         where ht.HouseTypeID == oldHousing.HouseTypeID
        //                                                         select ht.HousingType).SingleOrDefault(),
        //                                            DateTimeCreated = oldHousing.DateTimeCreated,
        //                                            NumOfView = oldHousing.NumOfView,
        //                                            NumOfSaved = oldHousing.NumOfSaved,
        //                                            NumOfPeople = oldHousing.NumOfPeople,
        //                                            NumOfRoom = oldHousing.NumOfRoom,
        //                                            NumOfBed = oldHousing.NumOfBed,
        //                                            NumOfBath = oldHousing.NumOfBath,
        //                                            AllowPet = oldHousing.AllowPet,
        //                                            HasWifi = oldHousing.HasWifi,
        //                                            HasAC = oldHousing.HasAC,
        //                                            HasParking = oldHousing.HasParking,
        //                                            TimeRestriction = oldHousing.TimeRestriction,
        //                                            Area = oldHousing.Area,
        //                                            Latitude = oldAddress.a.Latitude,
        //                                            Longitude = oldAddress.a.Longitude,
        //                                            AddressHouseNumber = oldAddress.a.HouseNumber,
        //                                            AddressStreet = oldAddress.a.Street,
        //                                            AddressWard = oldAddress.a.Ward,
        //                                            AddressDistrict = oldAddress.a.District,
        //                                            AddressCity = oldAddress.c.CityName,
        //                                            Description = oldHousing.Description,
        //                                            NumOfComment = oldHousing.NumOfComment
        //                                        },
        //                                        Creator = new UserModel
        //                                        {
        //                                            UserID = currentShareHousing.CreatorID,
        //                                            FirstName = currentShareHousingCreator.FirstName,
        //                                            LastName = currentShareHousingCreator.LastName,
        //                                            Email = (from user in db.GetTable<ShareSpaceUser>()
        //                                                     join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
        //                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
        //                                                     where user.UserID == currentShareHousingCreator.UserID
        //                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
        //                                            DOB = currentShareHousingCreator.DOB,
        //                                            PhoneNumber = currentShareHousingCreator.PhoneNumber,
        //                                            Gender = (from user in db.GetTable<ShareSpaceUser>()
        //                                                      join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
        //                                                      where user.UserID == currentShareHousingCreator.UserID
        //                                                      select g.GenderType).SingleOrDefault(),
        //                                            NumOfNote = currentShareHousingCreator.NumOfNote,
        //                                            DeviceTokens = currentShareHousingCreator.DeviceTokens
        //                                        },
        //                                        IsAvailable = currentShareHousing.IsAvailable,
        //                                        PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
        //                                        Description = currentShareHousing.Description,
        //                                        NumOfView = currentShareHousing.NumOfView,
        //                                        NumOfSaved = currentShareHousing.NumOfSaved,
        //                                        RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
        //                                        RequiredGender = (from g in db.GetTable<Gender>()
        //                                                          where g.GenderID == currentShareHousing.RequiredGenderID
        //                                                          select g.GenderType).SingleOrDefault(),
        //                                        RequiredWorkType = (from w in db.GetTable<Work>()
        //                                                            where w.WorkID == currentShareHousing.RequiredWorkID
        //                                                            select w.WorkType).SingleOrDefault(),
        //                                        AllowSmoking = currentShareHousing.AllowSmoking,
        //                                        AllowAlcohol = currentShareHousing.AllowAlcohol,
        //                                        HasPrivateKey = currentShareHousing.HasPrivateKey,
        //                                        DateTimeCreated = DateTimeOffset.UtcNow
        //                                    },
        //                                    SenderID = appointment.SenderID,
        //                                    RecipientID = appointment.RecipientID,
        //                                    AppointmentDateTime = appointment.AppointmentDateTime,
        //                                    DateTimeCreated = appointment.DateTimeCreated,
        //                                    Content = appointment.Content,
        //                                    IsOwnerConfirmed = appointment.IsCreatorConfirmed,
        //                                    IsUserConfirmed = appointment.IsUserConfirmed,
        //                                    NumOfRequests = appointment.NumOfRequests
        //                                });
        //                            }
        //                        }
        //                    }
        //                }
        //                if (appointments.Count > 0)
        //                {
        //                    return appointments;
        //                }
        //            }
        //        }
        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw e;
        //    }
        //}

        [Route("appointment/sharehousing")]
        [HttpGet]
        [Authorize]
        // Only User can get each Share Housing Appointment when open Share Housing Detail Activity.
        public ShareHousingAppointmentModel GetShareHousingAppointment(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where sh.HousingID == shareHousingID
                                               select sh).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                          where u.UserID == currentShareHousing.CreatorID
                                                          select u).SingleOrDefault();
                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                        if (currentShareHousingCreator != null && oldHousing != null)
                        {
                            var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == oldHousing.OwnerID
                                                   select u).SingleOrDefault();
                            var oldAddress = (from a in db.GetTable<Address>()
                                              join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                              join c in db.GetTable<City>() on a.CityID equals c.CityID
                                              where a.AddressID == oldHousing.AddressID
                                              select new { a, gl, c }).SingleOrDefault();
                            var oldShareHousingAppointment = (from ha in db.GetTable<ShareHousingAppointment>()
                                                              where (ha.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                 && (ha.SenderID == currentUser.UserID)
                                                                 && (ha.RecipientID == currentShareHousing.CreatorID)
                                                              select ha).SingleOrDefault();
                            if (oldHousingOwner != null && oldAddress != null && oldShareHousingAppointment != null)
                            {
                                return new ShareHousingAppointmentModel
                                {
                                    ShareHousing = new ShareHousingModel
                                    {
                                        ID = currentShareHousing.ShareHousingID,
                                        Housing = new HousingModel
                                        {
                                            ID = oldHousing.HousingID,
                                            PhotoURLs = (from a in db.GetTable<Album>()
                                                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                         where (a.HousingID == oldHousing.HousingID)
                                                            && (a.CreatorID == oldHousing.OwnerID)
                                                         select p.PhotoLink).ToList(),
                                            Title = oldHousing.Title,
                                            Owner = new UserModel
                                            {
                                                UserID = oldHousingOwner.UserID,
                                                FirstName = oldHousingOwner.FirstName,
                                                LastName = oldHousingOwner.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == oldHousingOwner.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = oldHousingOwner.DOB,
                                                PhoneNumber = oldHousingOwner.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == oldHousingOwner.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = oldHousingOwner.NumOfNote,
                                                DeviceTokens = oldHousingOwner.DeviceTokens
                                            },
                                            Price = oldHousing.Price,
                                            IsAvailable = oldHousing.IsAvailable,
                                            HouseType = (from ht in db.GetTable<HouseType>()
                                                         where ht.HouseTypeID == oldHousing.HouseTypeID
                                                         select ht.HousingType).SingleOrDefault(),
                                            DateTimeCreated = oldHousing.DateTimeCreated,
                                            NumOfView = oldHousing.NumOfView,
                                            NumOfSaved = oldHousing.NumOfSaved,
                                            NumOfPeople = oldHousing.NumOfPeople,
                                            NumOfRoom = oldHousing.NumOfRoom,
                                            NumOfBed = oldHousing.NumOfBed,
                                            NumOfBath = oldHousing.NumOfBath,
                                            AllowPet = oldHousing.AllowPet,
                                            HasWifi = oldHousing.HasWifi,
                                            HasAC = oldHousing.HasAC,
                                            HasParking = oldHousing.HasParking,
                                            TimeRestriction = oldHousing.TimeRestriction,
                                            Area = oldHousing.Area,
                                            Latitude = oldAddress.gl.Latitude,
                                            Longitude = oldAddress.gl.Longitude,
                                            AddressHouseNumber = oldAddress.a.HouseNumber,
                                            AddressStreet = oldAddress.a.Street,
                                            AddressWard = oldAddress.a.Ward,
                                            AddressDistrict = oldAddress.a.District,
                                            AddressCity = oldAddress.c.CityName,
                                            Description = oldHousing.Description,
                                            NumOfComment = oldHousing.NumOfComment
                                        },
                                        Creator = new UserModel
                                        {
                                            UserID = currentShareHousing.CreatorID,
                                            FirstName = currentShareHousingCreator.FirstName,
                                            LastName = currentShareHousingCreator.LastName,
                                            Email = (from user in db.GetTable<ShareSpaceUser>()
                                                     join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where user.UserID == currentShareHousingCreator.UserID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentShareHousingCreator.DOB,
                                            PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                            Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                      join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                      where user.UserID == currentShareHousingCreator.UserID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentShareHousingCreator.NumOfNote,
                                            DeviceTokens = currentShareHousingCreator.DeviceTokens
                                        },
                                        IsAvailable = currentShareHousing.IsAvailable,
                                        PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                        Description = currentShareHousing.Description,
                                        NumOfView = currentShareHousing.NumOfView,
                                        NumOfSaved = currentShareHousing.NumOfSaved,
                                        RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                        RequiredGender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == currentShareHousing.RequiredGenderID
                                                          select g.GenderType).SingleOrDefault(),
                                        RequiredWorkType = (from w in db.GetTable<Work>()
                                                            where w.WorkID == currentShareHousing.RequiredWorkID
                                                            select w.WorkType).SingleOrDefault(),
                                        AllowSmoking = currentShareHousing.AllowSmoking,
                                        AllowAlcohol = currentShareHousing.AllowAlcohol,
                                        HasPrivateKey = currentShareHousing.HasPrivateKey,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    },
                                    Sender = new UserModel
                                    {
                                        UserID = currentUser.UserID,
                                        FirstName = currentUser.FirstName,
                                        LastName = currentUser.LastName,
                                        Email = currentUser.Email,
                                        DOB = currentUser.DOB,
                                        PhoneNumber = currentUser.PhoneNumber,
                                        Gender = currentUser.Gender,
                                        NumOfNote = currentUser.NumOfNote,
                                        DeviceTokens = currentUser.DeviceTokens
                                    },
                                    RecipientID = oldShareHousingAppointment.RecipientID,
                                    AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                    DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                    Content = oldShareHousingAppointment.Content,
                                    IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                    IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                    NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                };
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

        [Route("appointment/sharehousing")]
        [HttpGet]
        [Authorize]
        // Only User can get each Share Housing Appointment when open Share Housing Detail Activity.
        public ShareHousingAppointmentModel GetShareHousingAppointment(int shareHousingID, int userID, int creatorID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where sh.HousingID == shareHousingID
                                               select sh).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                          where u.UserID == currentShareHousing.CreatorID
                                                          select u).SingleOrDefault();
                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                        if (currentShareHousingCreator != null && oldHousing != null)
                        {
                            var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                   where u.UserID == oldHousing.OwnerID
                                                   select u).SingleOrDefault();
                            var oldAddress = (from a in db.GetTable<Address>()
                                              join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                              join c in db.GetTable<City>() on a.CityID equals c.CityID
                                              where a.AddressID == oldHousing.AddressID
                                              select new { a, gl, c }).SingleOrDefault();
                            var oldShareHousingAppointment = (from ha in db.GetTable<ShareHousingAppointment>()
                                                              where (ha.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                 && (ha.SenderID == userID)
                                                                 && (ha.RecipientID == creatorID)
                                                              select ha).SingleOrDefault();
                            if (oldHousingOwner != null && oldAddress != null && oldShareHousingAppointment != null)
                            {
                                return new ShareHousingAppointmentModel
                                {
                                    ShareHousing = new ShareHousingModel
                                    {
                                        ID = currentShareHousing.ShareHousingID,
                                        Housing = new HousingModel
                                        {
                                            ID = oldHousing.HousingID,
                                            PhotoURLs = (from a in db.GetTable<Album>()
                                                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                         where (a.HousingID == oldHousing.HousingID)
                                                            && (a.CreatorID == oldHousing.OwnerID)
                                                         select p.PhotoLink).ToList(),
                                            Title = oldHousing.Title,
                                            Owner = new UserModel
                                            {
                                                UserID = oldHousingOwner.UserID,
                                                FirstName = oldHousingOwner.FirstName,
                                                LastName = oldHousingOwner.LastName,
                                                Email = (from e in db.GetTable<Email>()
                                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                         where e.EmailID == oldHousingOwner.EmailID
                                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                DOB = oldHousingOwner.DOB,
                                                PhoneNumber = oldHousingOwner.PhoneNumber,
                                                Gender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == oldHousingOwner.GenderID
                                                          select g.GenderType).SingleOrDefault(),
                                                NumOfNote = oldHousingOwner.NumOfNote,
                                                DeviceTokens = oldHousingOwner.DeviceTokens
                                            },
                                            Price = oldHousing.Price,
                                            IsAvailable = oldHousing.IsAvailable,
                                            HouseType = (from ht in db.GetTable<HouseType>()
                                                         where ht.HouseTypeID == oldHousing.HouseTypeID
                                                         select ht.HousingType).SingleOrDefault(),
                                            DateTimeCreated = oldHousing.DateTimeCreated,
                                            NumOfView = oldHousing.NumOfView,
                                            NumOfSaved = oldHousing.NumOfSaved,
                                            NumOfPeople = oldHousing.NumOfPeople,
                                            NumOfRoom = oldHousing.NumOfRoom,
                                            NumOfBed = oldHousing.NumOfBed,
                                            NumOfBath = oldHousing.NumOfBath,
                                            AllowPet = oldHousing.AllowPet,
                                            HasWifi = oldHousing.HasWifi,
                                            HasAC = oldHousing.HasAC,
                                            HasParking = oldHousing.HasParking,
                                            TimeRestriction = oldHousing.TimeRestriction,
                                            Area = oldHousing.Area,
                                            Latitude = oldAddress.gl.Latitude,
                                            Longitude = oldAddress.gl.Longitude,
                                            AddressHouseNumber = oldAddress.a.HouseNumber,
                                            AddressStreet = oldAddress.a.Street,
                                            AddressWard = oldAddress.a.Ward,
                                            AddressDistrict = oldAddress.a.District,
                                            AddressCity = oldAddress.c.CityName,
                                            Description = oldHousing.Description,
                                            NumOfComment = oldHousing.NumOfComment
                                        },
                                        Creator = new UserModel
                                        {
                                            UserID = currentShareHousing.CreatorID,
                                            FirstName = currentShareHousingCreator.FirstName,
                                            LastName = currentShareHousingCreator.LastName,
                                            Email = (from user in db.GetTable<ShareSpaceUser>()
                                                     join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                     where user.UserID == currentShareHousingCreator.UserID
                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                            DOB = currentShareHousingCreator.DOB,
                                            PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                            Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                      join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                      where user.UserID == currentShareHousingCreator.UserID
                                                      select g.GenderType).SingleOrDefault(),
                                            NumOfNote = currentShareHousingCreator.NumOfNote,
                                            DeviceTokens = currentShareHousingCreator.DeviceTokens
                                        },
                                        IsAvailable = currentShareHousing.IsAvailable,
                                        PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                        Description = currentShareHousing.Description,
                                        NumOfView = currentShareHousing.NumOfView,
                                        NumOfSaved = currentShareHousing.NumOfSaved,
                                        RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                        RequiredGender = (from g in db.GetTable<Gender>()
                                                          where g.GenderID == currentShareHousing.RequiredGenderID
                                                          select g.GenderType).SingleOrDefault(),
                                        RequiredWorkType = (from w in db.GetTable<Work>()
                                                            where w.WorkID == currentShareHousing.RequiredWorkID
                                                            select w.WorkType).SingleOrDefault(),
                                        AllowSmoking = currentShareHousing.AllowSmoking,
                                        AllowAlcohol = currentShareHousing.AllowAlcohol,
                                        HasPrivateKey = currentShareHousing.HasPrivateKey,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    },
                                    Sender = new UserModel
                                    {
                                        UserID = currentUser.UserID,
                                        FirstName = currentUser.FirstName,
                                        LastName = currentUser.LastName,
                                        Email = currentUser.Email,
                                        DOB = currentUser.DOB,
                                        PhoneNumber = currentUser.PhoneNumber,
                                        Gender = currentUser.Gender,
                                        NumOfNote = currentUser.NumOfNote,
                                        DeviceTokens = currentUser.DeviceTokens
                                    },
                                    RecipientID = oldShareHousingAppointment.RecipientID,
                                    AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                    DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                    Content = oldShareHousingAppointment.Content,
                                    IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                    IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                    NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                };
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

        [Route("appointment/sharehousing")]
        [HttpPost]
        [Authorize]
        // Current Sender is who sent this Create New appointment request. Recipient is who receive this request (Housing Owner in this case).
        public ShareHousingAppointmentModel SetNewShareHousingAppointment(int shareHousingID, int recipientID, string appointmentDateTime, string content)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    DateTimeOffset parsedDate = base.ParseDateFromString(appointmentDateTime);

                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (recipient != null)
                    {
                        // There is no case of Share Housing Creator Send New Appointment Request to User.
                        var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                                   where (sh.ShareHousingID == shareHousingID)
                                                      && (sh.CreatorID == recipient.UserID)
                                                   select sh).SingleOrDefault();
                        if (currentShareHousing != null)
                        {
                            var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                              where u.UserID == currentShareHousing.CreatorID
                                                              select u).SingleOrDefault();
                            var oldHousing = (from h in db.GetTable<Housing>()
                                              where h.HousingID == currentShareHousing.HousingID
                                              select h).SingleOrDefault();
                            if (currentShareHousingCreator != null && oldHousing != null)
                            {
                                var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                       where u.UserID == oldHousing.OwnerID
                                                       select u).SingleOrDefault();
                                var oldAddress = (from a in db.GetTable<Address>()
                                                  join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                  where a.AddressID == oldHousing.AddressID
                                                  select new { a, gl, c }).SingleOrDefault();
                                if (oldHousingOwner != null && oldAddress != null)
                                {
                                    var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                      where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                         && (sa.SenderID == currentSender.UserID)
                                                                         && (sa.RecipientID == currentShareHousing.CreatorID)
                                                                      select sa).SingleOrDefault();

                                    if (oldShareHousingAppointment != null)
                                    {
                                        // TODO: Edit to also cover update appointment condition.
                                        oldShareHousingAppointment.AppointmentDateTime = parsedDate;
                                        db.SubmitChanges();

                                        return new ShareHousingAppointmentModel
                                        {
                                            ShareHousing = new ShareHousingModel
                                            {
                                                ID = currentShareHousing.ShareHousingID,
                                                Housing = new HousingModel
                                                {
                                                    ID = oldHousing.HousingID,
                                                    PhotoURLs = (from a in db.GetTable<Album>()
                                                                 join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                 where (a.HousingID == oldHousing.HousingID)
                                                                    && (a.CreatorID == oldHousing.OwnerID)
                                                                 select p.PhotoLink).ToList(),
                                                    Title = oldHousing.Title,
                                                    Owner = new UserModel
                                                    {
                                                        UserID = oldHousingOwner.UserID,
                                                        FirstName = oldHousingOwner.FirstName,
                                                        LastName = oldHousingOwner.LastName,
                                                        Email = (from e in db.GetTable<Email>()
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where e.EmailID == oldHousingOwner.EmailID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = oldHousingOwner.DOB,
                                                        PhoneNumber = oldHousingOwner.PhoneNumber,
                                                        Gender = (from g in db.GetTable<Gender>()
                                                                  where g.GenderID == oldHousingOwner.GenderID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = oldHousingOwner.NumOfNote,
                                                        DeviceTokens = oldHousingOwner.DeviceTokens
                                                    },
                                                    Price = oldHousing.Price,
                                                    IsAvailable = oldHousing.IsAvailable,
                                                    HouseType = (from ht in db.GetTable<HouseType>()
                                                                 where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                 select ht.HousingType).SingleOrDefault(),
                                                    DateTimeCreated = oldHousing.DateTimeCreated,
                                                    NumOfView = oldHousing.NumOfView,
                                                    NumOfSaved = oldHousing.NumOfSaved,
                                                    NumOfPeople = oldHousing.NumOfPeople,
                                                    NumOfRoom = oldHousing.NumOfRoom,
                                                    NumOfBed = oldHousing.NumOfBed,
                                                    NumOfBath = oldHousing.NumOfBath,
                                                    AllowPet = oldHousing.AllowPet,
                                                    HasWifi = oldHousing.HasWifi,
                                                    HasAC = oldHousing.HasAC,
                                                    HasParking = oldHousing.HasParking,
                                                    TimeRestriction = oldHousing.TimeRestriction,
                                                    Area = oldHousing.Area,
                                                    Latitude = oldAddress.gl.Latitude,
                                                    Longitude = oldAddress.gl.Longitude,
                                                    AddressHouseNumber = oldAddress.a.HouseNumber,
                                                    AddressStreet = oldAddress.a.Street,
                                                    AddressWard = oldAddress.a.Ward,
                                                    AddressDistrict = oldAddress.a.District,
                                                    AddressCity = oldAddress.c.CityName,
                                                    Description = oldHousing.Description,
                                                    NumOfComment = oldHousing.NumOfComment
                                                },
                                                Creator = new UserModel
                                                {
                                                    UserID = currentShareHousing.CreatorID,
                                                    FirstName = currentShareHousingCreator.FirstName,
                                                    LastName = currentShareHousingCreator.LastName,
                                                    Email = (from user in db.GetTable<ShareSpaceUser>()
                                                             join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where user.UserID == currentShareHousingCreator.UserID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentShareHousingCreator.DOB,
                                                    PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                    Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                              join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                              where user.UserID == currentShareHousingCreator.UserID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentShareHousingCreator.NumOfNote,
                                                    DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                },
                                                IsAvailable = currentShareHousing.IsAvailable,
                                                PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                Description = currentShareHousing.Description,
                                                NumOfView = currentShareHousing.NumOfView,
                                                NumOfSaved = currentShareHousing.NumOfSaved,
                                                RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                RequiredGender = (from g in db.GetTable<Gender>()
                                                                  where g.GenderID == currentShareHousing.RequiredGenderID
                                                                  select g.GenderType).SingleOrDefault(),
                                                RequiredWorkType = (from w in db.GetTable<Work>()
                                                                    where w.WorkID == currentShareHousing.RequiredWorkID
                                                                    select w.WorkType).SingleOrDefault(),
                                                AllowSmoking = currentShareHousing.AllowSmoking,
                                                AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                DateTimeCreated = DateTimeOffset.UtcNow
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = currentSender.UserID,
                                                FirstName = currentSender.FirstName,
                                                LastName = currentSender.LastName,
                                                Email = currentSender.Email,
                                                DOB = currentSender.DOB,
                                                PhoneNumber = currentSender.PhoneNumber,
                                                Gender = currentSender.Gender,
                                                NumOfNote = currentSender.NumOfNote,
                                                DeviceTokens = currentSender.DeviceTokens
                                            },
                                            RecipientID = oldShareHousingAppointment.RecipientID,
                                            AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                            Content = oldShareHousingAppointment.Content,
                                            IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                            IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                        };
                                    }
                                    else
                                    {
                                        ShareHousingAppointment newShareHousingAppointment = new ShareHousingAppointment
                                        {
                                            ShareHousingID = currentShareHousing.HousingID,
                                            SenderID = currentSender.UserID,
                                            RecipientID = currentShareHousing.CreatorID,
                                            AppointmentDateTime = parsedDate,
                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                            Content = content,
                                            IsCreatorConfirmed = false,
                                            IsUserConfirmed = true,
                                            NumOfRequests = 1
                                        };
                                        db.ShareHousingAppointments.InsertOnSubmit(newShareHousingAppointment);
                                        db.SubmitChanges();

                                        return new ShareHousingAppointmentModel
                                        {
                                            ShareHousing = new ShareHousingModel
                                            {
                                                ID = currentShareHousing.ShareHousingID,
                                                Housing = new HousingModel
                                                {
                                                    ID = oldHousing.HousingID,
                                                    PhotoURLs = (from a in db.GetTable<Album>()
                                                                 join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                 where (a.HousingID == oldHousing.HousingID)
                                                                    && (a.CreatorID == oldHousing.OwnerID)
                                                                 select p.PhotoLink).ToList(),
                                                    Title = oldHousing.Title,
                                                    Owner = new UserModel
                                                    {
                                                        UserID = oldHousingOwner.UserID,
                                                        FirstName = oldHousingOwner.FirstName,
                                                        LastName = oldHousingOwner.LastName,
                                                        Email = (from e in db.GetTable<Email>()
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where e.EmailID == oldHousingOwner.EmailID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = oldHousingOwner.DOB,
                                                        PhoneNumber = oldHousingOwner.PhoneNumber,
                                                        Gender = (from g in db.GetTable<Gender>()
                                                                  where g.GenderID == oldHousingOwner.GenderID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = oldHousingOwner.NumOfNote,
                                                        DeviceTokens = oldHousingOwner.DeviceTokens
                                                    },
                                                    Price = oldHousing.Price,
                                                    IsAvailable = oldHousing.IsAvailable,
                                                    HouseType = (from ht in db.GetTable<HouseType>()
                                                                 where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                 select ht.HousingType).SingleOrDefault(),
                                                    DateTimeCreated = oldHousing.DateTimeCreated,
                                                    NumOfView = oldHousing.NumOfView,
                                                    NumOfSaved = oldHousing.NumOfSaved,
                                                    NumOfPeople = oldHousing.NumOfPeople,
                                                    NumOfRoom = oldHousing.NumOfRoom,
                                                    NumOfBed = oldHousing.NumOfBed,
                                                    NumOfBath = oldHousing.NumOfBath,
                                                    AllowPet = oldHousing.AllowPet,
                                                    HasWifi = oldHousing.HasWifi,
                                                    HasAC = oldHousing.HasAC,
                                                    HasParking = oldHousing.HasParking,
                                                    TimeRestriction = oldHousing.TimeRestriction,
                                                    Area = oldHousing.Area,
                                                    Latitude = oldAddress.gl.Latitude,
                                                    Longitude = oldAddress.gl.Longitude,
                                                    AddressHouseNumber = oldAddress.a.HouseNumber,
                                                    AddressStreet = oldAddress.a.Street,
                                                    AddressWard = oldAddress.a.Ward,
                                                    AddressDistrict = oldAddress.a.District,
                                                    AddressCity = oldAddress.c.CityName,
                                                    Description = oldHousing.Description,
                                                    NumOfComment = oldHousing.NumOfComment
                                                },
                                                Creator = new UserModel
                                                {
                                                    UserID = currentShareHousing.CreatorID,
                                                    FirstName = currentShareHousingCreator.FirstName,
                                                    LastName = currentShareHousingCreator.LastName,
                                                    Email = (from user in db.GetTable<ShareSpaceUser>()
                                                             join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where user.UserID == currentShareHousingCreator.UserID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = currentShareHousingCreator.DOB,
                                                    PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                    Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                              join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                              where user.UserID == currentShareHousingCreator.UserID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = currentShareHousingCreator.NumOfNote,
                                                    DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                },
                                                IsAvailable = currentShareHousing.IsAvailable,
                                                PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                Description = currentShareHousing.Description,
                                                NumOfView = currentShareHousing.NumOfView,
                                                NumOfSaved = currentShareHousing.NumOfSaved,
                                                RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                RequiredGender = (from g in db.GetTable<Gender>()
                                                                  where g.GenderID == currentShareHousing.RequiredGenderID
                                                                  select g.GenderType).SingleOrDefault(),
                                                RequiredWorkType = (from w in db.GetTable<Work>()
                                                                    where w.WorkID == currentShareHousing.RequiredWorkID
                                                                    select w.WorkType).SingleOrDefault(),
                                                AllowSmoking = currentShareHousing.AllowSmoking,
                                                AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                DateTimeCreated = DateTimeOffset.UtcNow
                                            },
                                            Sender = new UserModel
                                            {
                                                UserID = currentSender.UserID,
                                                FirstName = currentSender.FirstName,
                                                LastName = currentSender.LastName,
                                                Email = currentSender.Email,
                                                DOB = currentSender.DOB,
                                                PhoneNumber = currentSender.PhoneNumber,
                                                Gender = currentSender.Gender,
                                                NumOfNote = currentSender.NumOfNote,
                                                DeviceTokens = currentSender.DeviceTokens
                                            },
                                            RecipientID = newShareHousingAppointment.RecipientID,
                                            AppointmentDateTime = newShareHousingAppointment.AppointmentDateTime,
                                            DateTimeCreated = newShareHousingAppointment.DateTimeCreated,
                                            Content = newShareHousingAppointment.Content,
                                            IsOwnerConfirmed = newShareHousingAppointment.IsCreatorConfirmed,
                                            IsUserConfirmed = newShareHousingAppointment.IsUserConfirmed,
                                            NumOfRequests = newShareHousingAppointment.NumOfRequests
                                        };
                                    }
                                }
                                //var oldAddress = (from a in db.GetTable<Address>()
                                //                  join c in db.GetTable<City>() on a.CityID equals c.CityID
                                //                  where a.AddressID == currentHousing.AddressID
                                //                  select new { a, c }).SingleOrDefault();
                                //if (oldAddress != null)
                                //{
                                //    return new AppointmentModel
                                //    {
                                //        Housing = new HousingModel
                                //        {
                                //            ID = currentHousing.HousingID,
                                //            PhotoURLs = (from a in db.GetTable<Album>()
                                //                         join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                //                         where (a.HousingID == currentHousing.HousingID)
                                //                            && (a.CreatorID == currentHousing.OwnerID)
                                //                         select p.PhotoLink).ToList(),
                                //            Title = currentHousing.Title,
                                //            Owner = new UserModel
                                //            {
                                //                UserID = recipient.UserID,
                                //                FirstName = recipient.FirstName,
                                //                LastName = recipient.LastName,
                                //                Email = (from e in db.GetTable<Email>()
                                //                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                //                         where e.EmailID == recipient.EmailID
                                //                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                //                DOB = recipient.DOB,
                                //                PhoneNumber = recipient.PhoneNumber,
                                //                Gender = (from g in db.GetTable<Gender>()
                                //                          where g.GenderID == recipient.GenderID
                                //                          select g.GenderType).SingleOrDefault(),
                                //                NumOfNote = recipient.NumOfNote
                                //            },
                                //            Price = currentHousing.Price,
                                //            IsAvailable = currentHousing.IsAvailable,
                                //            HouseType = (from ht in db.GetTable<HouseType>()
                                //                         where ht.HouseTypeID == currentHousing.HouseTypeID
                                //                         select ht.HousingType).SingleOrDefault(),
                                //            DateTimeCreated = currentHousing.DateTimeCreated,
                                //            NumOfView = currentHousing.NumOfView,
                                //            NumOfSaved = currentHousing.NumOfSaved,
                                //            NumOfPeople = currentHousing.NumOfPeople,
                                //            NumOfRoom = currentHousing.NumOfRoom,
                                //            NumOfBed = currentHousing.NumOfBed,
                                //            NumOfBath = currentHousing.NumOfBath,
                                //            AllowPet = currentHousing.AllowPet,
                                //            HasWifi = currentHousing.HasWifi,
                                //            HasAC = currentHousing.HasAC,
                                //            HasParking = currentHousing.HasParking,
                                //            TimeRestriction = currentHousing.TimeRestriction,
                                //            Area = currentHousing.Area,
                                //            Latitude = oldAddress.a.Latitude,
                                //            Longitude = oldAddress.a.Longitude,
                                //            AddressHouseNumber = oldAddress.a.HouseNumber,
                                //            AddressStreet = oldAddress.a.Street,
                                //            AddressWard = oldAddress.a.Ward,
                                //            AddressDistrict = oldAddress.a.District,
                                //            AddressCity = oldAddress.c.CityName,
                                //            Description = currentHousing.Description,
                                //            NumOfComment = currentHousing.NumOfComment
                                //        },
                                //        SenderID = currentSender.UserID,
                                //        RecipientID = currentHousing.OwnerID,
                                //        AppointmentDateTime = appointmentDateTime
                                //    };
                                //}
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

        [Route("appointment/sharehousing")]
        [HttpPut]
        [Authorize]
        // Current Sender is who sent this Update request. Recipient is who receive this request.
        public ShareHousingAppointmentModel UpdateShareHousingAppointment(int shareHousingID, int recipientID, string appointmentDateTime, String content)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    DateTimeOffset parsedDate = base.ParseDateFromString(appointmentDateTime);

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Share Housing Creator and User can send Update request.
                        var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                                   where (sh.ShareHousingID == shareHousingID)
                                                   select sh).SingleOrDefault();
                        if (currentShareHousing != null)
                        {
                            // Share Housing Creator is sending this Update request.
                            if (sender.UserID == currentShareHousing.CreatorID)
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == recipient.UserID)    // => User is recipient.
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            oldShareHousingAppointment.AppointmentDateTime = parsedDate;
                                            oldShareHousingAppointment.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousingAppointment.Content = content;
                                            oldShareHousingAppointment.IsCreatorConfirmed = true;
                                            oldShareHousingAppointment.IsUserConfirmed = false;
                                            oldShareHousingAppointment.NumOfRequests = ++oldShareHousingAppointment.NumOfRequests;
                                            db.SubmitChanges();

                                            return new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = recipient.UserID,
                                                    FirstName = recipient.FirstName,
                                                    LastName = recipient.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == recipient.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = recipient.DOB,
                                                    PhoneNumber = recipient.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == recipient.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = recipient.NumOfNote,
                                                    DeviceTokens = recipient.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                        }
                                    }
                                }
                            }
                            else if (recipient.UserID == currentShareHousing.CreatorID)    // User is sending this Update request.
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == sender.UserID)
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)    // => Share Housing Creator is recipient.
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            oldShareHousingAppointment.AppointmentDateTime = parsedDate;
                                            oldShareHousingAppointment.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousingAppointment.Content = content;
                                            oldShareHousingAppointment.IsCreatorConfirmed = false;
                                            oldShareHousingAppointment.IsUserConfirmed = true;
                                            oldShareHousingAppointment.NumOfRequests = ++oldShareHousingAppointment.NumOfRequests;
                                            db.SubmitChanges();

                                            return new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = sender.UserID,
                                                    FirstName = sender.FirstName,
                                                    LastName = sender.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == sender.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = sender.DOB,
                                                    PhoneNumber = sender.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == sender.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = sender.NumOfNote,
                                                    DeviceTokens = sender.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                        }
                                    }
                                }
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

        [Route("appointment/sharehousing")]
        [HttpDelete]
        [Authorize]
        // Current Sender is who sent this Delete request. Recipient is who receive this request.
        public ShareHousingAppointmentModel DeleteShareHousingAppointment(int shareHousingID, int recipientID)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Share Housing Creator and User can send Delete request.
                        var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                                   where (sh.ShareHousingID == shareHousingID)
                                                   select sh).SingleOrDefault();
                        if (currentShareHousing != null)
                        {
                            // Share Housing Creator is sending this Delete request.
                            if (sender.UserID == currentShareHousing.CreatorID)
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == recipient.UserID)    // => User is recipient.
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            ShareHousingAppointmentModel sa = new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = recipient.UserID,
                                                    FirstName = recipient.FirstName,
                                                    LastName = recipient.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == recipient.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = recipient.DOB,
                                                    PhoneNumber = recipient.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == recipient.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = recipient.NumOfNote,
                                                    DeviceTokens = recipient.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                            db.ShareHousingAppointments.DeleteOnSubmit(oldShareHousingAppointment);
                                            db.SubmitChanges();
                                            return sa;
                                        }
                                    }
                                }
                            }
                            else if (recipient.UserID == currentShareHousing.CreatorID)    // User is sending this Delete request.
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == sender.UserID)
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)    // => Share Housing Creator is recipient.
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            ShareHousingAppointmentModel sa = new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = sender.UserID,
                                                    FirstName = sender.FirstName,
                                                    LastName = sender.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == sender.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = sender.DOB,
                                                    PhoneNumber = sender.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == sender.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = sender.NumOfNote,
                                                    DeviceTokens = sender.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                            db.ShareHousingAppointments.DeleteOnSubmit(oldShareHousingAppointment);
                                            db.SubmitChanges();
                                            return sa;
                                        }
                                    }
                                }
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

        [Route("appointment/sharehousing")]
        [HttpPut]
        [Authorize]
        // Current Sender is who sent this Accept request. Recipient is who receive this request.
        public ShareHousingAppointmentModel AcceptShareHousingAppointment(int shareHousingID, int recipientID)
        {
            try
            {
                UserModel currentSender = base.GetCurrentUserInfo();

                if (currentSender != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var sender = (from u in db.GetTable<ShareSpaceUser>()
                                  where u.UserID == currentSender.UserID
                                  select u).SingleOrDefault();
                    var recipient = (from u in db.GetTable<ShareSpaceUser>()
                                     where u.UserID == recipientID
                                     select u).SingleOrDefault();
                    if (sender != null && recipient != null)
                    {
                        // Both Share Housing Creator and User can send Accept request.
                        // Only User has right to create an (send a new request of) appointment.
                        // => Only from 2nd update request (Share Housing Creator send update request to User),
                        //    User has right to accept the appointment.

                        var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                                   where (sh.ShareHousingID == shareHousingID)
                                                   select sh).SingleOrDefault();
                        if (currentShareHousing != null)
                        {
                            // Share Housing Creator is sending this Accept request.
                            if (sender.UserID == currentShareHousing.CreatorID)
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == recipient.UserID)    // => User is recipient.
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            oldShareHousingAppointment.IsCreatorConfirmed = true;
                                            oldShareHousingAppointment.IsUserConfirmed = true;
                                            oldShareHousingAppointment.NumOfRequests = ++oldShareHousingAppointment.NumOfRequests;
                                            db.SubmitChanges();

                                            return new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = recipient.UserID,
                                                    FirstName = recipient.FirstName,
                                                    LastName = recipient.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == recipient.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = recipient.DOB,
                                                    PhoneNumber = recipient.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == recipient.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = recipient.NumOfNote,
                                                    DeviceTokens = recipient.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                        }
                                    }
                                }
                            }
                            else if (recipient.UserID == currentShareHousing.CreatorID)    // User is sending this Accept request.
                            {
                                var currentShareHousingCreator = (from u in db.GetTable<ShareSpaceUser>()
                                                                  where u.UserID == currentShareHousing.CreatorID
                                                                  select u).SingleOrDefault();
                                var oldHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                                if (currentShareHousingCreator != null && oldHousing != null)
                                {
                                    var oldHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                           where u.UserID == oldHousing.OwnerID
                                                           select u).SingleOrDefault();
                                    var oldAddress = (from a in db.GetTable<Address>()
                                                      join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                      join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                      where a.AddressID == oldHousing.AddressID
                                                      select new { a, gl, c }).SingleOrDefault();
                                    if (oldHousingOwner != null && oldAddress != null)
                                    {
                                        var oldShareHousingAppointment = (from sa in db.GetTable<ShareHousingAppointment>()
                                                                          where (sa.ShareHousingID == currentShareHousing.ShareHousingID)
                                                                             && (sa.SenderID == sender.UserID)
                                                                             && (sa.RecipientID == currentShareHousing.CreatorID)    // => Share Housing Creator is recipient.
                                                                          select sa).SingleOrDefault();
                                        if (oldShareHousingAppointment != null)
                                        {
                                            oldShareHousingAppointment.IsCreatorConfirmed = true;
                                            oldShareHousingAppointment.IsUserConfirmed = true;
                                            oldShareHousingAppointment.NumOfRequests = ++oldShareHousingAppointment.NumOfRequests;
                                            db.SubmitChanges();

                                            return new ShareHousingAppointmentModel
                                            {
                                                ShareHousing = new ShareHousingModel
                                                {
                                                    ID = currentShareHousing.ShareHousingID,
                                                    Housing = new HousingModel
                                                    {
                                                        ID = oldHousing.HousingID,
                                                        PhotoURLs = (from a in db.GetTable<Album>()
                                                                     join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                     where (a.HousingID == oldHousing.HousingID)
                                                                        && (a.CreatorID == oldHousing.OwnerID)
                                                                     select p.PhotoLink).ToList(),
                                                        Title = oldHousing.Title,
                                                        Owner = new UserModel
                                                        {
                                                            UserID = oldHousingOwner.UserID,
                                                            FirstName = oldHousingOwner.FirstName,
                                                            LastName = oldHousingOwner.LastName,
                                                            Email = (from e in db.GetTable<Email>()
                                                                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                     where e.EmailID == oldHousingOwner.EmailID
                                                                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                            DOB = oldHousingOwner.DOB,
                                                            PhoneNumber = oldHousingOwner.PhoneNumber,
                                                            Gender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == oldHousingOwner.GenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                            NumOfNote = oldHousingOwner.NumOfNote,
                                                            DeviceTokens = oldHousingOwner.DeviceTokens
                                                        },
                                                        Price = oldHousing.Price,
                                                        IsAvailable = oldHousing.IsAvailable,
                                                        HouseType = (from ht in db.GetTable<HouseType>()
                                                                     where ht.HouseTypeID == oldHousing.HouseTypeID
                                                                     select ht.HousingType).SingleOrDefault(),
                                                        DateTimeCreated = oldHousing.DateTimeCreated,
                                                        NumOfView = oldHousing.NumOfView,
                                                        NumOfSaved = oldHousing.NumOfSaved,
                                                        NumOfPeople = oldHousing.NumOfPeople,
                                                        NumOfRoom = oldHousing.NumOfRoom,
                                                        NumOfBed = oldHousing.NumOfBed,
                                                        NumOfBath = oldHousing.NumOfBath,
                                                        AllowPet = oldHousing.AllowPet,
                                                        HasWifi = oldHousing.HasWifi,
                                                        HasAC = oldHousing.HasAC,
                                                        HasParking = oldHousing.HasParking,
                                                        TimeRestriction = oldHousing.TimeRestriction,
                                                        Area = oldHousing.Area,
                                                        Latitude = oldAddress.gl.Latitude,
                                                        Longitude = oldAddress.gl.Longitude,
                                                        AddressHouseNumber = oldAddress.a.HouseNumber,
                                                        AddressStreet = oldAddress.a.Street,
                                                        AddressWard = oldAddress.a.Ward,
                                                        AddressDistrict = oldAddress.a.District,
                                                        AddressCity = oldAddress.c.CityName,
                                                        Description = oldHousing.Description,
                                                        NumOfComment = oldHousing.NumOfComment
                                                    },
                                                    Creator = new UserModel
                                                    {
                                                        UserID = currentShareHousing.CreatorID,
                                                        FirstName = currentShareHousingCreator.FirstName,
                                                        LastName = currentShareHousingCreator.LastName,
                                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                                 where user.UserID == currentShareHousingCreator.UserID
                                                                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                        DOB = currentShareHousingCreator.DOB,
                                                        PhoneNumber = currentShareHousingCreator.PhoneNumber,
                                                        Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                                  where user.UserID == currentShareHousingCreator.UserID
                                                                  select g.GenderType).SingleOrDefault(),
                                                        NumOfNote = currentShareHousingCreator.NumOfNote,
                                                        DeviceTokens = currentShareHousingCreator.DeviceTokens
                                                    },
                                                    IsAvailable = currentShareHousing.IsAvailable,
                                                    PricePerMonthOfOne = currentShareHousing.PricePerMonthOfOne,
                                                    Description = currentShareHousing.Description,
                                                    NumOfView = currentShareHousing.NumOfView,
                                                    NumOfSaved = currentShareHousing.NumOfSaved,
                                                    RequiredNumOfPeople = currentShareHousing.RequiredNumOfPeople,
                                                    RequiredGender = (from g in db.GetTable<Gender>()
                                                                      where g.GenderID == currentShareHousing.RequiredGenderID
                                                                      select g.GenderType).SingleOrDefault(),
                                                    RequiredWorkType = (from w in db.GetTable<Work>()
                                                                        where w.WorkID == currentShareHousing.RequiredWorkID
                                                                        select w.WorkType).SingleOrDefault(),
                                                    AllowSmoking = currentShareHousing.AllowSmoking,
                                                    AllowAlcohol = currentShareHousing.AllowAlcohol,
                                                    HasPrivateKey = currentShareHousing.HasPrivateKey,
                                                    DateTimeCreated = DateTimeOffset.UtcNow
                                                },
                                                Sender = new UserModel
                                                {
                                                    UserID = sender.UserID,
                                                    FirstName = sender.FirstName,
                                                    LastName = sender.LastName,
                                                    Email = (from e in db.GetTable<Email>()
                                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                             where e.EmailID == sender.EmailID
                                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                                    DOB = sender.DOB,
                                                    PhoneNumber = sender.PhoneNumber,
                                                    Gender = (from g in db.GetTable<Gender>()
                                                              where g.GenderID == sender.GenderID
                                                              select g.GenderType).SingleOrDefault(),
                                                    NumOfNote = sender.NumOfNote,
                                                    DeviceTokens = sender.DeviceTokens
                                                },
                                                RecipientID = oldShareHousingAppointment.RecipientID,
                                                AppointmentDateTime = oldShareHousingAppointment.AppointmentDateTime,
                                                DateTimeCreated = oldShareHousingAppointment.DateTimeCreated,
                                                Content = oldShareHousingAppointment.Content,
                                                IsOwnerConfirmed = oldShareHousingAppointment.IsCreatorConfirmed,
                                                IsUserConfirmed = oldShareHousingAppointment.IsUserConfirmed,
                                                NumOfRequests = oldShareHousingAppointment.NumOfRequests
                                            };
                                        }
                                    }
                                }
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

        [Route("post/housing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfPostedHousings(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (h.OwnerID == currentUser.UserID)
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                             select h).ToList();

                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        return olderHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/housing")]
        [HttpGet]
        [Authorize]
        public List<HousingModel> GetMoreOlderPostedHousings(string currentBottomHousingDateTimeCreated = null)
        {
            try
            {
                List<HousingModel> olderHousings = new List<HousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (h.OwnerID == currentUser.UserID)
                                && (h.DateTimeCreated < parsedDate)
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                             orderby h.DateTimeCreated descending
                             select new HousingModel
                             {
                                 ID = h.HousingID,
                                 Title = h.Title,
                                 Owner = new UserModel
                                 {
                                     UserID = h.OwnerID,
                                     UID = u.UID,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == h.OwnerID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u.DOB,
                                     PhoneNumber = u.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == h.OwnerID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u.NumOfNote,
                                     DeviceTokens = u.DeviceTokens
                                 },
                                 Price = h.Price,
                                 IsAvailable = h.IsAvailable,
                                 HouseType = ht.HousingType,
                                 DateTimeCreated = h.DateTimeCreated,
                                 NumOfView = h.NumOfView,
                                 NumOfSaved = h.NumOfSaved,
                                 NumOfPeople = h.NumOfPeople,
                                 NumOfRoom = h.NumOfRoom,
                                 NumOfBed = h.NumOfBed,
                                 NumOfBath = h.NumOfBath,
                                 AllowPet = h.AllowPet,
                                 HasWifi = h.HasWifi,
                                 HasAC = h.HasAC,
                                 HasParking = h.HasParking,
                                 TimeRestriction = h.TimeRestriction,
                                 Area = h.Area,
                                 Latitude = gl.Latitude,
                                 Longitude = gl.Longitude,
                                 AddressHouseNumber = a.HouseNumber,
                                 AddressStreet = a.Street,
                                 AddressWard = a.Ward,
                                 AddressDistrict = a.District,
                                 AddressCity = c.CityName,
                                 Description = h.Description,
                                 //LatestCommentContent = "",
                                 NumOfComment = h.NumOfComment,
                                 //AuthorizationValue = nameFilter
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastHousing =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (h.OwnerID == currentUser.UserID)
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                             orderby h.DateTimeCreated descending
                             select new HousingModel
                             {
                                 ID = h.HousingID,
                                 Title = h.Title,
                                 Owner = new UserModel
                                 {
                                     UserID = h.OwnerID,
                                     UID = u.UID,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == h.OwnerID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u.DOB,
                                     PhoneNumber = u.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == h.OwnerID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u.NumOfNote,
                                     DeviceTokens = u.DeviceTokens
                                 },
                                 Price = h.Price,
                                 IsAvailable = h.IsAvailable,
                                 HouseType = ht.HousingType,
                                 DateTimeCreated = h.DateTimeCreated,
                                 NumOfView = h.NumOfView,
                                 NumOfSaved = h.NumOfSaved,
                                 NumOfPeople = h.NumOfPeople,
                                 NumOfRoom = h.NumOfRoom,
                                 NumOfBed = h.NumOfBed,
                                 NumOfBath = h.NumOfBath,
                                 AllowPet = h.AllowPet,
                                 HasWifi = h.HasWifi,
                                 HasAC = h.HasAC,
                                 HasParking = h.HasParking,
                                 TimeRestriction = h.TimeRestriction,
                                 Area = h.Area,
                                 Latitude = gl.Latitude,
                                 Longitude = gl.Longitude,
                                 AddressHouseNumber = a.HouseNumber,
                                 AddressStreet = a.Street,
                                 AddressWard = a.Ward,
                                 AddressDistrict = a.District,
                                 AddressCity = c.CityName,
                                 Description = h.Description,
                                 //LatestCommentContent = "",
                                 NumOfComment = h.NumOfComment,
                                 //AuthorizationValue = nameFilter
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();

                        if (lastHousing != null)
                        {
                            olderHousings.Add(lastHousing);
                        }
                    }
                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        foreach (var item in olderHousings)
                        {
                            item.PhotoURLs = (from a in db.GetTable<Album>()
                                              join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                              where (a.HousingID == item.ID)
                                                 && (a.CreatorID == item.Owner.UserID)
                                              orderby p.PhotoID
                                              select p.PhotoLink).ToList();
                        }
                    }
                    return olderHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/housing/availability")]
        [HttpGet]
        [Authorize]
        public bool GetHidingStateOfCurrentHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where (h.HousingID == housingID)
                                             && (h.OwnerID == currentUser.UserID)
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        if (!currentHousing.IsAvailable)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/housing/availability")]
        [HttpPut]
        [Authorize]
        public bool UnhideHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where (h.HousingID == housingID)
                                             && (h.OwnerID == currentUser.UserID)
                                          select h).SingleOrDefault();
                    var currentShareHousings = (from sh in db.GetTable<ShareHousing>()
                                                where sh.HousingID == housingID
                                                select sh).ToList();
                    if (currentHousing != null)
                    {
                        if (!currentHousing.IsAvailable)
                        {
                            currentHousing.IsAvailable = true;

                            if (currentShareHousings != null && currentShareHousings.Count > 0)
                            {
                                foreach (var shareHousing in currentShareHousings)
                                {
                                    shareHousing.IsAvailable = true;
                                }
                            }
                        }
                        db.SubmitChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/housing/availability")]
        [HttpDelete]
        [Authorize]
        public bool HideHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where (h.HousingID == housingID)
                                             && (h.OwnerID == currentUser.UserID)
                                          select h).SingleOrDefault();
                    var currentShareHousings = (from sh in db.GetTable<ShareHousing>()
                                                where sh.HousingID == housingID
                                                select sh).ToList();
                    if (currentHousing != null)
                    {
                        if (currentHousing.IsAvailable)
                        {
                            currentHousing.IsAvailable = false;

                            if (currentShareHousings != null && currentShareHousings.Count > 0)
                            {
                                foreach (var shareHousing in currentShareHousings)
                                {
                                    shareHousing.IsAvailable = false;
                                }
                            }
                        }
                        db.SubmitChanges();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/sharehousing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfPostedShareHousings(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (sh.CreatorID == currentUser.UserID)
                                && ((h.OwnerID != currentUser.UserID && h.IsAvailable == true)
                                    || (h.OwnerID == currentUser.UserID))
                             select sh).ToList();

                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        return olderShareHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<ShareHousingModel> GetMoreOlderPostedShareHousings(string currentBottomShareHousingDateTimeCreated = null)
        {
            try
            {
                List<ShareHousingModel> olderShareHousings = new List<ShareHousingModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomShareHousingDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (sh.CreatorID == currentUser.UserID)
                                && (sh.DateTimeCreated < parsedDate)
                                && ((h.OwnerID != currentUser.UserID && h.IsAvailable == true)
                                    || (h.OwnerID == currentUser.UserID))
                             orderby sh.DateTimeCreated descending
                             select new ShareHousingModel
                             {
                                 ID = sh.ShareHousingID,
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u1.UID,
                                         FirstName = u1.FirstName,
                                         LastName = u1.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u1.DOB,
                                         PhoneNumber = u1.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u1.NumOfNote,
                                         DeviceTokens = u1.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     NumOfComment = h.NumOfComment,
                                 },
                                 Creator = new UserModel
                                 {
                                     UserID = sh.CreatorID,
                                     FirstName = u2.FirstName,
                                     LastName = u2.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == sh.CreatorID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u2.DOB,
                                     PhoneNumber = u2.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == sh.CreatorID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u2.NumOfNote,
                                     DeviceTokens = u2.DeviceTokens
                                 },
                                 IsAvailable = sh.IsAvailable,
                                 PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                 Description = sh.Description,
                                 NumOfView = sh.NumOfView,
                                 NumOfSaved = sh.NumOfSaved,
                                 RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                 RequiredGender = (from g in db.GetTable<Gender>()
                                                   where g.GenderID == sh.RequiredGenderID
                                                   select g.GenderType).SingleOrDefault(),
                                 RequiredWorkType = (from w in db.GetTable<Work>()
                                                     where w.WorkID == sh.RequiredWorkID
                                                     select w.WorkType).SingleOrDefault(),
                                 AllowSmoking = sh.AllowSmoking,
                                 AllowAlcohol = sh.AllowAlcohol,
                                 HasPrivateKey = sh.HasPrivateKey,
                                 DateTimeCreated = DateTimeOffset.UtcNow
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastShareHousing =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (sh.CreatorID == currentUser.UserID)
                                && ((h.OwnerID != currentUser.UserID && h.IsAvailable == true)
                                    || (h.OwnerID == currentUser.UserID))
                             orderby sh.DateTimeCreated descending
                             select new ShareHousingModel
                             {
                                 ID = sh.ShareHousingID,
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u1.UID,
                                         FirstName = u1.FirstName,
                                         LastName = u1.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u1.DOB,
                                         PhoneNumber = u1.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u1.NumOfNote,
                                         DeviceTokens = u1.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     NumOfComment = h.NumOfComment,
                                 },
                                 Creator = new UserModel
                                 {
                                     UserID = sh.CreatorID,
                                     FirstName = u2.FirstName,
                                     LastName = u2.LastName,
                                     Email = (from user in db.GetTable<ShareSpaceUser>()
                                              join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                              join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                              where user.UserID == sh.CreatorID
                                              select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                     DOB = u2.DOB,
                                     PhoneNumber = u2.PhoneNumber,
                                     Gender = (from user in db.GetTable<ShareSpaceUser>()
                                               join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                               where user.UserID == sh.CreatorID
                                               select g.GenderType).SingleOrDefault(),
                                     NumOfNote = u2.NumOfNote,
                                     DeviceTokens = u2.DeviceTokens
                                 },
                                 IsAvailable = sh.IsAvailable,
                                 PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                 Description = sh.Description,
                                 NumOfView = sh.NumOfView,
                                 NumOfSaved = sh.NumOfSaved,
                                 RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                 RequiredGender = (from g in db.GetTable<Gender>()
                                                   where g.GenderID == sh.RequiredGenderID
                                                   select g.GenderType).SingleOrDefault(),
                                 RequiredWorkType = (from w in db.GetTable<Work>()
                                                     where w.WorkID == sh.RequiredWorkID
                                                     select w.WorkType).SingleOrDefault(),
                                 AllowSmoking = sh.AllowSmoking,
                                 AllowAlcohol = sh.AllowAlcohol,
                                 HasPrivateKey = sh.HasPrivateKey,
                                 DateTimeCreated = DateTimeOffset.UtcNow
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();
                        if (lastShareHousing != null)
                        {
                            olderShareHousings.Add(lastShareHousing);
                        }
                    }
                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        foreach (var item in olderShareHousings)
                        {
                            item.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                      join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                      where (a.HousingID == item.Housing.ID)
                                                         && (a.CreatorID == item.Housing.Owner.UserID)
                                                      orderby p.PhotoID
                                                      select p.PhotoLink).ToList();
                        }
                    }
                    return olderShareHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/sharehousing/availability")]
        [HttpGet]
        [Authorize]
        public bool GetHidingStateOfCurrentShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where (sh.ShareHousingID == shareHousingID)
                                                  && (sh.CreatorID == currentUser.UserID)
                                               select sh).SingleOrDefault();
                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        if (currentShareHousing != null)
                        {
                            if (!currentShareHousing.IsAvailable)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/sharehousing/availability")]
        [HttpPut]
        [Authorize]
        public bool UnhideShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where (sh.ShareHousingID == shareHousingID)
                                                  && (sh.CreatorID == currentUser.UserID)
                                               select sh).SingleOrDefault();
                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        if (currentShareHousing != null)
                        {
                            if (!currentShareHousing.IsAvailable)
                            {
                                currentShareHousing.IsAvailable = true;

                                if (currentShareHousing.CreatorID == currentHousing.OwnerID)
                                {
                                    currentHousing.IsAvailable = true;
                                }
                            }
                            db.SubmitChanges();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("post/sharehousing/availability")]
        [HttpDelete]
        [Authorize]
        public bool HideShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where (sh.ShareHousingID == shareHousingID)
                                                  && (sh.CreatorID == currentUser.UserID)
                                               select sh).SingleOrDefault();
                    var currentHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == currentShareHousing.HousingID
                                          select h).SingleOrDefault();
                    if (currentHousing != null)
                    {
                        if (currentShareHousing != null)
                        {
                            if (currentShareHousing.IsAvailable)
                            {
                                currentShareHousing.IsAvailable = false;

                                if (currentShareHousing.CreatorID == currentHousing.OwnerID)
                                {
                                    currentHousing.IsAvailable = false;
                                }
                            }
                            db.SubmitChanges();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("note/housing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfHousingsWithNotes(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join n in db.GetTable<Note>() on h.HousingID equals n.HousingID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == -1)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             select h).ToList();

                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        return olderHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("note/housing")]
        [HttpGet]
        [Authorize]
        public List<HistoryHousingNoteModel> GetMoreOlderHousingsWithNotes(string currentBottomHousingNoteDateTimeCreated = null)
        {
            try
            {
                List<HistoryHousingNoteModel> olderHousings = new List<HistoryHousingNoteModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomHousingNoteDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join n in db.GetTable<Note>() on h.HousingID equals n.HousingID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == -1)
                                && (n.DateTimeCreated < parsedDate)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby n.DateTimeCreated descending
                             select new HistoryHousingNoteModel
                             {
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = n.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastHousing =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join n in db.GetTable<Note>() on h.HousingID equals n.HousingID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == -1)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby n.DateTimeCreated descending
                             select new HistoryHousingNoteModel
                             {
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = n.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();

                        if (lastHousing != null)
                        {
                            olderHousings.Add(lastHousing);
                        }
                    }
                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        foreach (var item in olderHousings)
                        {
                            item.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                      join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                      where (a.HousingID == item.Housing.ID)
                                                         && (a.CreatorID == item.Housing.Owner.UserID)
                                                      orderby p.PhotoID
                                                      select p.PhotoLink).ToList();
                        }
                    }
                    return olderHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("note/sharehousing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfShareHousingsWithNotes(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();
                    
                    var olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join n in db.GetTable<Note>() on sh.HousingID equals n.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == sh.ShareHousingID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             select sh).ToList();

                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        return olderShareHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("note/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<HistoryShareHousingNoteModel> GetMoreOlderShareHousingsWithNotes(string currentBottomShareHousingNoteDateTimeCreated = null)
        {
            try
            {
                List<HistoryShareHousingNoteModel> olderShareHousings = new List<HistoryShareHousingNoteModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomShareHousingNoteDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join n in db.GetTable<Note>() on sh.HousingID equals n.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == sh.ShareHousingID)
                                && (n.DateTimeCreated < parsedDate)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby n.DateTimeCreated descending
                             select new HistoryShareHousingNoteModel
                             {
                                 ShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = n.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastShareHousing =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join n in db.GetTable<Note>() on sh.HousingID equals n.HousingID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (n.CreatorID == currentUser.UserID)
                                && (n.ShareHousingID == sh.ShareHousingID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby n.DateTimeCreated descending
                             select new HistoryShareHousingNoteModel
                             {
                                 ShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = n.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();
                        if (lastShareHousing != null)
                        {
                            olderShareHousings.Add(lastShareHousing);
                        }
                    }
                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        foreach (var item in olderShareHousings)
                        {
                            item.ShareHousing.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                                   join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                   where (a.HousingID == item.ShareHousing.Housing.ID)
                                                                      && (a.CreatorID == item.ShareHousing.Housing.Owner.UserID)
                                                                   orderby p.PhotoID
                                                                   select p.PhotoLink).ToList();
                        }
                    }
                    return olderShareHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("photo/housing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfHousingsWithPhotos(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.ShareHousingID == -1)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             select h).ToList();

                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        return olderHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("photo/housing")]
        [HttpGet]
        [Authorize]
        public List<HistoryHousingPhotoModel> GetMoreOlderHousingsWithPhotos(string currentBottomHousingPhotoDateTimeCreated = null)
        {
            try
            {
                List<HistoryHousingPhotoModel> olderHousings = new List<HistoryHousingPhotoModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomHousingPhotoDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderHousings =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.ShareHousingID == -1)
                                && (ab.DateTimeCreated < parsedDate)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ab.DateTimeCreated descending
                             select new HistoryHousingPhotoModel
                             {
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = ab.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastHousing =
                            (from h in db.GetTable<Housing>()
                             join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (h.DateTimeCreated < DateTimeOffset.UtcNow)
                             //   &&
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.ShareHousingID == -1)
                                && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                            // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                                && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                            // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ab.DateTimeCreated descending
                             select new HistoryHousingPhotoModel
                             {
                                 Housing = new HousingModel
                                 {
                                     ID = h.HousingID,
                                     Title = h.Title,
                                     Owner = new UserModel
                                     {
                                         UserID = h.OwnerID,
                                         UID = u.UID,
                                         FirstName = u.FirstName,
                                         LastName = u.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == h.OwnerID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u.DOB,
                                         PhoneNumber = u.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == h.OwnerID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u.NumOfNote,
                                         DeviceTokens = u.DeviceTokens
                                     },
                                     Price = h.Price,
                                     IsAvailable = h.IsAvailable,
                                     HouseType = ht.HousingType,
                                     DateTimeCreated = h.DateTimeCreated,
                                     NumOfView = h.NumOfView,
                                     NumOfSaved = h.NumOfSaved,
                                     NumOfPeople = h.NumOfPeople,
                                     NumOfRoom = h.NumOfRoom,
                                     NumOfBed = h.NumOfBed,
                                     NumOfBath = h.NumOfBath,
                                     AllowPet = h.AllowPet,
                                     HasWifi = h.HasWifi,
                                     HasAC = h.HasAC,
                                     HasParking = h.HasParking,
                                     TimeRestriction = h.TimeRestriction,
                                     Area = h.Area,
                                     Latitude = gl.Latitude,
                                     Longitude = gl.Longitude,
                                     AddressHouseNumber = a.HouseNumber,
                                     AddressStreet = a.Street,
                                     AddressWard = a.Ward,
                                     AddressDistrict = a.District,
                                     AddressCity = c.CityName,
                                     Description = h.Description,
                                     //LatestCommentContent = "",
                                     NumOfComment = h.NumOfComment,
                                     //AuthorizationValue = nameFilter
                                 },
                                 DateTimeCreated = ab.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();

                        if (lastHousing != null)
                        {
                            olderHousings.Add(lastHousing);
                        }
                    }
                    if (olderHousings != null && olderHousings.Count > 0)
                    {
                        foreach (var item in olderHousings)
                        {
                            item.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                      join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                      where (a.HousingID == item.Housing.ID)
                                                         && (a.CreatorID == item.Housing.Owner.UserID)
                                                      orderby p.PhotoID
                                                      select p.PhotoLink).ToList();
                        }
                    }
                    return olderHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("photo/sharehousing")]
        [HttpGet]
        [Authorize]
        public int GetNumOfShareHousingsWithPhotos(int dummyParam)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.CreatorID != sh.CreatorID)
                                && (ab.ShareHousingID == sh.ShareHousingID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby sh.ShareHousingID descending
                             select sh).ToList();

                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        return olderShareHousings.Count;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("photo/sharehousing")]
        [HttpGet]
        [Authorize]
        public List<HistoryShareHousingPhotoModel> GetMoreOlderShareHousingsWithPhotos(string currentBottomShareHousingPhotoDateTimeCreated = null)
        {
            try
            {
                List<HistoryShareHousingPhotoModel> olderShareHousings = new List<HistoryShareHousingPhotoModel>();

                UserModel currentUser = base.GetCurrentUserInfo();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomShareHousingPhotoDateTimeCreated);

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    if (parsedDate != null)
                    {
                        olderShareHousings =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.CreatorID != sh.CreatorID)
                                && (ab.ShareHousingID == sh.ShareHousingID)
                                && (ab.DateTimeCreated < parsedDate)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ab.DateTimeCreated descending
                             select new HistoryShareHousingPhotoModel
                             {
                                 ShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = ab.DateTimeCreated
                             }).Take(5).ToList();
                        //}).Skip(5 * offset).Take(5).ToList();
                    }
                    else
                    {
                        var lastShareHousing =
                            (from sh in db.GetTable<ShareHousing>()
                             join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                             join ab in db.GetTable<Album>() on h.HousingID equals ab.HousingID
                             //join p in db.GetTable<Photo>() on ab.AlbumID equals p.AlbumID
                             join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                             join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                             join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                             join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                             join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                             join c in db.GetTable<City>() on a.CityID equals c.CityID
                             //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                             //where (sh.DateTimeCreated < DateTimeOffset.UtcNow)
                             where (ab.CreatorID == currentUser.UserID)
                                && (ab.CreatorID != h.OwnerID)
                                && (ab.CreatorID != sh.CreatorID)
                                && (ab.ShareHousingID == sh.ShareHousingID)
                                && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                                && (h.IsAvailable == true)
                                && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                             orderby ab.DateTimeCreated descending
                             select new HistoryShareHousingPhotoModel
                             {
                                 ShareHousing = new ShareHousingModel
                                 {
                                     ID = sh.ShareHousingID,
                                     Housing = new HousingModel
                                     {
                                         ID = h.HousingID,
                                         Title = h.Title,
                                         Owner = new UserModel
                                         {
                                             UserID = h.OwnerID,
                                             UID = u1.UID,
                                             FirstName = u1.FirstName,
                                             LastName = u1.LastName,
                                             Email = (from user in db.GetTable<ShareSpaceUser>()
                                                      join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                      join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                      where user.UserID == h.OwnerID
                                                      select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                             DOB = u1.DOB,
                                             PhoneNumber = u1.PhoneNumber,
                                             Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                       join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                       where user.UserID == h.OwnerID
                                                       select g.GenderType).SingleOrDefault(),
                                             NumOfNote = u1.NumOfNote,
                                             DeviceTokens = u1.DeviceTokens
                                         },
                                         Price = h.Price,
                                         IsAvailable = h.IsAvailable,
                                         HouseType = ht.HousingType,
                                         DateTimeCreated = h.DateTimeCreated,
                                         NumOfView = h.NumOfView,
                                         NumOfSaved = h.NumOfSaved,
                                         NumOfPeople = h.NumOfPeople,
                                         NumOfRoom = h.NumOfRoom,
                                         NumOfBed = h.NumOfBed,
                                         NumOfBath = h.NumOfBath,
                                         AllowPet = h.AllowPet,
                                         HasWifi = h.HasWifi,
                                         HasAC = h.HasAC,
                                         HasParking = h.HasParking,
                                         TimeRestriction = h.TimeRestriction,
                                         Area = h.Area,
                                         Latitude = gl.Latitude,
                                         Longitude = gl.Longitude,
                                         AddressHouseNumber = a.HouseNumber,
                                         AddressStreet = a.Street,
                                         AddressWard = a.Ward,
                                         AddressDistrict = a.District,
                                         AddressCity = c.CityName,
                                         Description = h.Description,
                                         NumOfComment = h.NumOfComment,
                                     },
                                     Creator = new UserModel
                                     {
                                         UserID = sh.CreatorID,
                                         FirstName = u2.FirstName,
                                         LastName = u2.LastName,
                                         Email = (from user in db.GetTable<ShareSpaceUser>()
                                                  join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                  join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                  where user.UserID == sh.CreatorID
                                                  select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                         DOB = u2.DOB,
                                         PhoneNumber = u2.PhoneNumber,
                                         Gender = (from user in db.GetTable<ShareSpaceUser>()
                                                   join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                                   where user.UserID == sh.CreatorID
                                                   select g.GenderType).SingleOrDefault(),
                                         NumOfNote = u2.NumOfNote,
                                         DeviceTokens = u2.DeviceTokens
                                     },
                                     IsAvailable = sh.IsAvailable,
                                     PricePerMonthOfOne = sh.PricePerMonthOfOne,
                                     Description = sh.Description,
                                     NumOfView = sh.NumOfView,
                                     NumOfSaved = sh.NumOfSaved,
                                     RequiredNumOfPeople = sh.RequiredNumOfPeople,
                                     RequiredGender = (from g in db.GetTable<Gender>()
                                                       where g.GenderID == sh.RequiredGenderID
                                                       select g.GenderType).SingleOrDefault(),
                                     RequiredWorkType = (from w in db.GetTable<Work>()
                                                         where w.WorkID == sh.RequiredWorkID
                                                         select w.WorkType).SingleOrDefault(),
                                     AllowSmoking = sh.AllowSmoking,
                                     AllowAlcohol = sh.AllowAlcohol,
                                     HasPrivateKey = sh.HasPrivateKey,
                                     DateTimeCreated = DateTimeOffset.UtcNow
                                 },
                                 DateTimeCreated = ab.DateTimeCreated
                             }).Take(1).SingleOrDefault();
                        //}).Skip(5 * offset).Take(5).ToList();
                        if (lastShareHousing != null)
                        {
                            olderShareHousings.Add(lastShareHousing);
                        }
                    }
                    if (olderShareHousings != null && olderShareHousings.Count > 0)
                    {
                        foreach (var item in olderShareHousings)
                        {
                            item.ShareHousing.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                                   join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                                   where (a.HousingID == item.ShareHousing.Housing.ID)
                                                                      && (a.CreatorID == item.ShareHousing.Housing.Owner.UserID)
                                                                   orderby p.PhotoID
                                                                   select p.PhotoLink).ToList();
                        }
                    }
                    return olderShareHousings;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
