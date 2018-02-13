using ShareSpaceServer.DBMapping;
using ShareSpaceServer.Models;
using ShareSpaceServer.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Device.Location;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShareSpaceServer.Controllers
{
    [RoutePrefix("api/housing")]
    public class HousingController : BaseController
    {
        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        public List<HousingModel> GetMoreOlderHousings(string currentBottomHousingDateTimeCreated = null)  //, int offset)
        {
            try
            {
                //IEnumerable<string> headerValues;
                //var nameFilter = string.Empty;
                //if (Request.Headers.TryGetValues("Authorization", out headerValues))
                //{
                //    nameFilter = headerValues.FirstOrDefault();
                //}
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();
                //return (from e in db.GetTable<Email>()
                //        orderby e.EmailID descending
                //        select new HousingModel
                //        {
                //            DomainID = e.DomainID,
                //            LocalPart = e.LocalPart,
                //            EmailID = e.EmailID
                //        }).Skip(3).Take(3).ToList();
                List<HousingModel> olderHousings = new List<HousingModel>();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomHousingDateTimeCreated);
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
                         where (h.DateTimeCreated < parsedDate)
                            && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                        // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                            && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                        // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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
                         where (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                        // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                            && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                        // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        public List<HousingModel> GetMoreNewerHousings(string currentTopHousingDateTimeCreated)
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                DateTimeOffset parsedDate = base.ParseDateFromString(currentTopHousingDateTimeCreated);

                List<HousingModel> newerHousings =
                    (from h in db.GetTable<Housing>()
                     join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                     join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                     join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                     join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                     join c in db.GetTable<City>() on a.CityID equals c.CityID
                     //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                     where (h.DateTimeCreated > parsedDate)
                        && (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                    // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                        && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                    // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                        && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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

                if (newerHousings != null && newerHousings.Count > 0)
                {
                    foreach (var item in newerHousings)
                    {
                        item.PhotoURLs = (from a in db.GetTable<Album>()
                                          join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                          where (a.HousingID == item.ID)
                                             && (a.CreatorID == item.Owner.UserID)
                                          orderby p.PhotoID
                                          select p.PhotoLink).ToList();
                    }
                }
                return newerHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("search")]
        [HttpPost]
        public List<HousingModel> SearchHousings(SearchHousingDataModel data)
        {
            try
            {
                // Location Criteria.
                //MyGeoLocation geoLocation = MyGeoLocation.FromDegrees((double)data.Latitude, (double)data.Longitude);
                //MyGeoLocation[] boudingCoordinates = geoLocation.BoundingCoordinates(data.Radius);

                // Price Criteria.
                int[] priceRange = new int[2];
                if (data.MinPrice == -2)    // Any.
                {
                    if (data.MaxPrice == -2)    // Any.
                    {
                        priceRange[0] = -2;
                        priceRange[1] = -2;
                    }
                    else if (data.MaxPrice == -1)   // Deal => No such case.
                    {
                        return null;
                    }
                    else    // Other Max Prices.
                    {
                        priceRange[0] = 0;
                        priceRange[1] = data.MaxPrice;
                    }
                }
                else if (data.MinPrice == -1)   // Deal.
                {
                    if (data.MaxPrice == -1)    // Deal.
                    {
                        priceRange[0] = -1;
                        priceRange[1] = -1;
                    }
                    else    // Not Deal => No such case.
                    {
                        return null;
                    }
                }
                else    // Other Min Prices.
                {
                    if (data.MaxPrice == -2)    // Any.
                    {
                        priceRange[0] = data.MinPrice;
                        priceRange[1] = 1000000000;
                    }
                    else if (data.MaxPrice == -1)   // Deal => No such case.
                    {
                        return null;
                    }
                    else    // Other Max Prices.
                    {
                        priceRange[0] = data.MinPrice;
                        priceRange[1] = data.MaxPrice;
                    }
                }

                // Area Criteria.
                decimal[] areaRange = new decimal[2];
                if (data.MinArea == -1)     // Any.
                {
                    if (data.MaxArea == -1)     // Any.
                    {
                        areaRange[0] = 0;
                        areaRange[1] = 1000000000;
                    }
                    else    // Other Max Areas.
                    {
                        areaRange[0] = 0;
                        areaRange[1] = data.MaxArea;
                    }
                }
                else    // Other Min Areas.
                {
                    if (data.MaxArea == -1)     // Any
                    {
                        areaRange[0] = data.MinArea;
                        areaRange[1] = 1000000000;
                    }
                    else    // Other Max Areas.
                    {
                        areaRange[0] = data.MinArea;
                        areaRange[1] = data.MaxArea;
                    }
                }

                // Time Restriction Criteria.
                TimeSpan[] timeRestrictionRange = new TimeSpan[2];
                if (String.IsNullOrEmpty(data.MinTimeRestriction))  // Any
                {
                    if (String.IsNullOrEmpty(data.MaxTimeRestriction))  // Any
                    {
                        timeRestrictionRange[0] = new TimeSpan(0, 0, 0);
                        timeRestrictionRange[1] = new TimeSpan(23, 59, 59);
                    }
                    else    // Other Max Time Restrictions.
                    {
                        timeRestrictionRange[0] = new TimeSpan(0, 0, 0);
                        timeRestrictionRange[1] = TimeSpan.ParseExact(data.MaxTimeRestriction, "hh\\:mm", null);
                    }
                }
                else    // Other Min Time Restrictions.
                {
                    if (String.IsNullOrEmpty(data.MaxTimeRestriction))  // Any
                    {
                        timeRestrictionRange[0] = TimeSpan.ParseExact(data.MinTimeRestriction, "hh\\:mm", null);
                        timeRestrictionRange[1] = new TimeSpan(23, 59, 59);
                    }
                    else    // Other Max Time Restrictions.
                    {
                        timeRestrictionRange[0] = TimeSpan.ParseExact(data.MinTimeRestriction, "hh\\:mm", null);
                        timeRestrictionRange[1] = TimeSpan.ParseExact(data.MaxTimeRestriction, "hh\\:mm", null);
                    }
                }

                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                var resultHousingsQuery =
                        (from h in db.GetTable<Housing>()
                         join u in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u.UserID
                         join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                         join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                         join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                         join c in db.GetTable<City>() on a.CityID equals c.CityID
                         where
                            //   (boudingCoordinates[0].getLatitudeInDegrees() <= (double)gl.Latitude && (double)gl.Latitude <= boudingCoordinates[1].getLatitudeInDegrees())
                            //&& (boudingCoordinates[0].getLongitudeInDegrees() <= (double)gl.Longitude && (double)gl.Longitude <= boudingCoordinates[1].getLongitudeInDegrees())
                            //&& ((data.HouseTypes[0]) ? (0 <= h.HouseTypeID) : (houseTypeDBIndex.Any(index => h.HouseTypeID == index)))
                            //&& 
                               (((priceRange[0] != -2 && priceRange[1] != -2) && (priceRange[0] != -1 && priceRange[1] != -1))
                               ? (priceRange[0] <= h.Price && h.Price <= priceRange[1])
                               : ((priceRange[0] == -1 && priceRange[1] == -1) ? h.Price == -1 : -1 <= h.Price))
                            && (areaRange[0] <= h.Area && h.Area <= areaRange[1])
                            && (data.NumPeople != -1 ? data.NumPeople <= h.NumOfPeople : -1 <= h.NumOfPeople)
                            && (data.NumRoom != -1 ? data.NumRoom <= h.NumOfRoom : -1 <= h.NumOfRoom)
                            && (data.NumBed != -1 ? data.NumBed <= h.NumOfBed : -1 <= h.NumOfBed)
                            && (data.NumBath != -1 ? data.NumBath <= h.NumOfBath : -1 <= h.NumOfBath)
                            //&& (data.Amenities[0] ? (h.AllowPet == false || h.AllowPet == true) : (h.AllowPet == amenities[0] && h.HasWifi == amenities[1] && h.HasAC == amenities[2] && h.HasParking == amenities[3]))
                            && ((timeRestrictionRange[0] == new TimeSpan(0, 0, 0) && timeRestrictionRange[1] == new TimeSpan(23, 59, 59))
                               ? true
                               : timeRestrictionRange[0] <= h.TimeRestriction && h.TimeRestriction <= timeRestrictionRange[1])
                            &&
                               (h.IsAvailable == true)  // IsAvailable == true <=> This housing was posted in public mode OR Housing's Owner press show it on Housing Feed.
                                                    // IsAvailable == false <=> This housing was posted in private mode OR Housing's Owner press hide it when it is rented.
                            && (h.IsExist == true)      // IsExist == true <=> This Housing was posted by Housing's Owner.
                                                        // IsExist == false <=> This Housing was posted along with Share Housing Info by Normal Users finding roommate to share. => Don't show it on Housing Feed.
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                         orderby h.DateTimeCreated descending
                         //select new HousingModel
                         //{
                         //    ID = h.HousingID,
                         //    Title = h.Title,
                         //    Owner = new UserModel
                         //    {
                         //        UserID = h.OwnerID,
                         //        UID = u.UID,
                         //        FirstName = u.FirstName,
                         //        LastName = u.LastName,
                         //        Email = (from user in db.GetTable<ShareSpaceUser>()
                         //                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                         //                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                         //                 where user.UserID == h.OwnerID
                         //                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                         //        DOB = u.DOB,
                         //        PhoneNumber = u.PhoneNumber,
                         //        Gender = (from user in db.GetTable<ShareSpaceUser>()
                         //                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                         //                  where user.UserID == h.OwnerID
                         //                  select g.GenderType).SingleOrDefault(),
                         //        NumOfNote = u.NumOfNote,
                         //        DeviceTokens = u.DeviceTokens
                         //    },
                         //    Price = h.Price,
                         //    IsAvailable = h.IsAvailable,
                         //    HouseType = ht.HousingType,
                         //    DateTimeCreated = h.DateTimeCreated,
                         //    NumOfView = h.NumOfView,
                         //    NumOfSaved = h.NumOfSaved,
                         //    NumOfPeople = h.NumOfPeople,
                         //    NumOfRoom = h.NumOfRoom,
                         //    NumOfBed = h.NumOfBed,
                         //    NumOfBath = h.NumOfBath,
                         //    AllowPet = h.AllowPet,
                         //    HasWifi = h.HasWifi,
                         //    HasAC = h.HasAC,
                         //    HasParking = h.HasParking,
                         //    TimeRestriction = h.TimeRestriction,
                         //    Area = h.Area,
                         //    Latitude = gl.Latitude,
                         //    Longitude = gl.Longitude,
                         //    AddressHouseNumber = a.HouseNumber,
                         //    AddressStreet = a.Street,
                         //    AddressWard = a.Ward,
                         //    AddressDistrict = a.District,
                         //    AddressCity = c.CityName,
                         //    Description = h.Description,
                         //    //LatestCommentContent = "",
                         //    NumOfComment = h.NumOfComment,
                         //    //AuthorizationValue = nameFilter
                         //}
                         select new { h, u, ht, a, gl, c });

                // Where Clause for House Type Criteria.
                //if (data.HouseTypes[0])
                //{
                //    resultHousingsQuery = resultHousingQuery.Where(item => 0 <= item.h.HouseTypeID);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingQuery.Where(item => houseTypeDBIndex.Any(index => item.h.HouseTypeID == index));
                //}
                //if (!data.HouseTypes[0])
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => houseTypeDBIndex.Any(index => item.h.HouseTypeID == index));
                //}

                // Where Clause for Price Criteria.
                //if ((priceRange[0] != -2 && priceRange[1] != -2) && (priceRange[0] != -1 && priceRange[1] != -1))
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => priceRange[0] <= item.h.Price && item.h.Price <= priceRange[1]);
                //}
                //else if (priceRange[0] == -1 && priceRange[1] == -1)
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => item.h.Price == -1);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => -1 <= item.h.Price);
                //}

                // Where Clause for Num People Criteria.
                //if (data.NumPeople != -1)
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => data.NumPeople <= item.h.NumOfPeople);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => -1 <= item.h.NumOfPeople);
                //}

                // Where Clause for Num Room Criteria.
                //if (data.NumRoom != -1)
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => data.NumRoom <= item.h.NumOfRoom);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => -1 <= item.h.NumOfRoom);
                //}

                // Where Clause for Num Bed Criteria.
                //if (data.NumBed != -1)
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => data.NumBed <= item.h.NumOfBed);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => -1 <= item.h.NumOfBed);
                //}

                // Where Clause for Num Bath Criteria.
                //if (data.NumBath != -1)
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => data.NumBath <= item.h.NumOfBath);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => -1 <= item.h.NumOfBath);
                //}

                // Where Clause for Amenities Criteria.
                //if (data.Amenities[0])
                //{
                //    resultHousingsQuery = resultHousingQuery.Where(item => (item.h.AllowPet == false || item.h.AllowPet == true));
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingQuery.Where(item => 
                //          (item.h.AllowPet == amenities[0]
                //        && item.h.HasWifi == amenities[1]
                //        && item.h.HasAC == amenities[2]
                //        && item.h.HasParking == amenities[3])
                //    );
                //}
                if (!data.Amenities[0])
                {
                    // Amenities Criteria.
                    bool[] amenities = new bool[4];
                    for (int i = 0; i < data.Amenities.Length; ++i)
                    {
                        if (i != 0)
                        {
                            amenities[i - 1] = data.Amenities[i];
                        }
                    }
                    for (int i = 0; i < amenities.Length; ++i)
                    {
                        if (i == 0 && amenities[0])
                        {
                            resultHousingsQuery = resultHousingsQuery.Where(item => item.h.AllowPet == amenities[0]);
                        }
                        else if (i == 1 && amenities[1])
                        {
                            resultHousingsQuery = resultHousingsQuery.Where(item => item.h.HasWifi == amenities[1]);
                        }
                        else if (i == 2 && amenities[2])
                        {
                            resultHousingsQuery = resultHousingsQuery.Where(item => item.h.HasAC == amenities[2]);
                        }
                        else if (i == 3 && amenities[3])
                        {
                            resultHousingsQuery = resultHousingsQuery.Where(item => item.h.HasParking == amenities[3]);
                        }
                    }
                }

                // Where Clause for Time Restriction Criteria.
                //if (timeRestrictionRange[0] == new TimeSpan(0, 0, 0) && timeRestrictionRange[1] == new TimeSpan(23, 59, 59))
                //{
                //    //resultHousingsQuery = resultHousingQuery.Where(item => true);
                //}
                //else
                //{
                //    resultHousingsQuery = resultHousingsQuery.Where(item => timeRestrictionRange[0] <= item.h.TimeRestriction && item.h.TimeRestriction <= timeRestrictionRange[1]);
                //}

                List<HousingModel> resultHousings =
                    resultHousingsQuery.Select(
                        item => new HousingModel
                        {
                            ID = item.h.HousingID,
                            Title = item.h.Title,
                            Owner = new UserModel
                            {
                                UserID = item.h.OwnerID,
                                UID = item.u.UID,
                                FirstName = item.u.FirstName,
                                LastName = item.u.LastName,
                                Email = (from user in db.GetTable<ShareSpaceUser>()
                                         join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                         where user.UserID == item.h.OwnerID
                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                DOB = item.u.DOB,
                                PhoneNumber = item.u.PhoneNumber,
                                Gender = (from user in db.GetTable<ShareSpaceUser>()
                                          join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                          where user.UserID == item.h.OwnerID
                                          select g.GenderType).SingleOrDefault(),
                                NumOfNote = item.u.NumOfNote,
                                DeviceTokens = item.u.DeviceTokens
                            },
                            Price = item.h.Price,
                            IsAvailable = item.h.IsAvailable,
                            HouseType = item.ht.HousingType,
                            DateTimeCreated = item.h.DateTimeCreated,
                            NumOfView = item.h.NumOfView,
                            NumOfSaved = item.h.NumOfSaved,
                            NumOfPeople = item.h.NumOfPeople,
                            NumOfRoom = item.h.NumOfRoom,
                            NumOfBed = item.h.NumOfBed,
                            NumOfBath = item.h.NumOfBath,
                            AllowPet = item.h.AllowPet,
                            HasWifi = item.h.HasWifi,
                            HasAC = item.h.HasAC,
                            HasParking = item.h.HasParking,
                            TimeRestriction = item.h.TimeRestriction,
                            Area = item.h.Area,
                            Latitude = item.gl.Latitude,
                            Longitude = item.gl.Longitude,
                            AddressHouseNumber = item.a.HouseNumber,
                            AddressStreet = item.a.Street,
                            AddressWard = item.a.Ward,
                            AddressDistrict = item.a.District,
                            AddressCity = item.c.CityName,
                            Description = item.h.Description,
                            //LatestCommentContent = "",
                            NumOfComment = item.h.NumOfComment
                            //AuthorizationValue = nameFilter
                        }).ToList();

                if (resultHousings != null && resultHousings.Count > 0)
                {
                    List<int> removeIndexes = new List<int>();

                    // Keywords Criteria.
                    if (!String.IsNullOrEmpty(data.Keywords))
                    {
                        var keywordsPunctuation = data.Keywords.ToLower().Where(Char.IsPunctuation).Distinct().ToArray();
                        string[] splitKeywords = data.Keywords.ToLower().Split().Select(x => x.Trim(keywordsPunctuation)).ToArray();

                        for (int i = 0; i < resultHousings.Count; ++i)
                        {
                            string title = resultHousings.ElementAt(i).Title.ToLower();
                            var titlePunctuation = title.Where(Char.IsPunctuation).Distinct().ToArray();
                            string[] titleWords = title.Split().Select(x => x.Trim(titlePunctuation)).ToArray();

                            string description = resultHousings.ElementAt(i).Description.ToLower();
                            var descriptionPunctuation = description.Where(Char.IsPunctuation).Distinct().ToArray();
                            string[] descriptionWords = description.Split().Select(x => x.Trim(descriptionPunctuation)).ToArray();

                            if (!titleWords.Any(w => splitKeywords.Contains(w))
                                && !descriptionWords.Any(w => splitKeywords.Contains(w)))
                            {
                                removeIndexes.Add(i);
                            }
                        }
                        foreach (var index in removeIndexes.OrderByDescending(item => item))
                        {
                            resultHousings.RemoveAt(index);
                        }
                    }

                    // House Type Criteria.
                    if (!data.HouseTypes[0])
                    {
                        if (removeIndexes != null)
                        {
                            if (removeIndexes.Count > 0)
                            {
                                removeIndexes.Clear();
                            }
                            //int[] houseTypeDBIndex = new int[4];    // Array of 4 zero elements.
                            List<String> houseTypes = new List<String>();
                            for (int i = 0; i < data.HouseTypes.Length; ++i)
                            {
                                if (i != 0 && data.HouseTypes[i])
                                {
                                    string houseType = (from ht in db.GetTable<HouseType>()
                                                        where ht.HouseTypeID == i
                                                        select ht.HousingType).SingleOrDefault();
                                    if (!String.IsNullOrEmpty(houseType))
                                    {
                                        houseTypes.Add(houseType);
                                    }
                                }
                            }
                            for (int i = 0; i < resultHousings.Count; ++i)
                            {
                                if (!houseTypes.Any(type => resultHousings.ElementAt(i).HouseType == type))
                                {
                                    removeIndexes.Add(i);
                                }
                            }
                            foreach (var index in removeIndexes.OrderByDescending(item => item))
                            {
                                resultHousings.RemoveAt(index);
                            }
                        }
                    }

                    // Location Criteria.
                    if (removeIndexes != null)
                    {
                        if (removeIndexes.Count > 0)
                        {
                            removeIndexes.Clear();
                        }
                        for (int i = 0; i < resultHousings.Count; ++i)
                        {
                            GeoCoordinate currentHousingGeoCoordinate = new GeoCoordinate(
                                    (double) resultHousings.ElementAt(i).Latitude,
                                    (double) resultHousings.ElementAt(i).Longitude
                                );
                            GeoCoordinate searchDataGeoCoordinate = new GeoCoordinate(
                                    (double) data.Latitude,
                                    (double) data.Longitude
                                );
                            if (currentHousingGeoCoordinate.GetDistanceTo(searchDataGeoCoordinate) > data.Radius)
                            {
                                removeIndexes.Add(i);
                            }
                        }
                        foreach (var index in removeIndexes.OrderByDescending(item => item))
                        {
                            resultHousings.RemoveAt(index);
                        }
                    }

                    foreach (var item in resultHousings)
                    {
                        item.PhotoURLs = (from a in db.GetTable<Album>()
                                          join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                          where (a.HousingID == item.ID)
                                             && (a.CreatorID == item.Owner.UserID)
                                          orderby p.PhotoID
                                          select p.PhotoLink).ToList();
                    }
                }
                return resultHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("")]
        [HttpPost]
        [Authorize]
        public HousingModel PostHousing(HousingModel housing)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var city = (from c in db.GetTable<City>()
                                where c.CityName == housing.AddressCity
                                select c).SingleOrDefault();
                    if (city != null)
                    {
                        var oldAddress = (from a in db.GetTable<Address>()
                                          where (object.Equals(a.HouseNumber, housing.AddressHouseNumber))
                                             && (object.Equals(a.Street, housing.AddressStreet))
                                             && (object.Equals(a.Ward, housing.AddressWard))
                                             && (a.District == housing.AddressDistrict)
                                             && (a.CityID == city.CityID)
                                          select a).SingleOrDefault();

                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where (h.HousingID == housing.ID)
                                             && (h.OwnerID == currentUser.UserID)
                                          select h).SingleOrDefault();
                        if (oldAddress != null)
                        {
                            if (oldHousing != null)
                            {
                                var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                      where gl.LocationID == oldAddress.LocationID
                                                      select gl).SingleOrDefault();
                                //if (oldGeoLocation != null)
                                //{
                                //    oldGeoLocation.Latitude = housing.Latitude;
                                //    oldGeoLocation.Longitude = housing.Longitude;
                                //    db.SubmitChanges();
                                //}
                                //else
                                if (oldGeoLocation == null)
                                {
                                    GeoLocation newGeoLocation = new GeoLocation
                                    {
                                        Latitude = housing.Latitude,
                                        Longitude = housing.Longitude
                                    };
                                    db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                    db.SubmitChanges();

                                    oldAddress.LocationID = newGeoLocation.LocationID;
                                    db.SubmitChanges();
                                }

                                // oldAddress exists. oldHousing exists. Update existent Housing.
                                oldHousing.Title = housing.Title;
                                oldHousing.Price = housing.Price;
                                oldHousing.IsAvailable = housing.IsAvailable;
                                oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                          where ht.HousingType == housing.HouseType
                                                          select ht.HouseTypeID).SingleOrDefault();
                                oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                oldHousing.DateTimeExpired = null;
                                oldHousing.NumOfPeople = housing.NumOfPeople;
                                oldHousing.NumOfRoom = housing.NumOfRoom;
                                oldHousing.NumOfBed = housing.NumOfBed;
                                oldHousing.NumOfBath = housing.NumOfBath;
                                oldHousing.AllowPet = housing.AllowPet;
                                oldHousing.HasWifi = housing.HasWifi;
                                oldHousing.HasAC = housing.HasAC;
                                oldHousing.HasParking = housing.HasParking;
                                oldHousing.TimeRestriction = housing.TimeRestriction;
                                oldHousing.Area = housing.Area;

                                oldHousing.AddressID = oldAddress.AddressID;

                                oldHousing.Description = housing.Description;

                                // Update existent Album.
                                var oldAlbum = (from a in db.GetTable<Album>()
                                                where (a.HousingID == oldHousing.HousingID)
                                                   && (a.CreatorID == currentUser.UserID)
                                                select a).SingleOrDefault();
                                if (oldAlbum != null)
                                {
                                    oldAlbum.AlbumName = housing.Title;
                                    oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                    // Update existent Photos.
                                    var oldPhotos = (from p in db.GetTable<Photo>()
                                                     where p.AlbumID == oldAlbum.AlbumID
                                                     orderby p.PhotoID
                                                     select p).ToList();
                                    if (oldPhotos != null && oldPhotos.Count > 0)
                                    {
                                        // US culture.
                                        var usCulture = new CultureInfo("en-US");
                                        if (housing.PhotoURLs.Count == oldPhotos.Count)
                                        {
                                            for (int i = 0; i < oldPhotos.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                        }
                                        else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                        {
                                            int i = 0;
                                            for (; i < oldPhotos.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                            List<Photo> newPhotos = new List<Photo>();
                                            for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                Photo newPhoto = new Photo
                                                {
                                                    PhotoName = photoName,
                                                    DateTimeCreated = DateTimeOffset.UtcNow,
                                                    AlbumID = oldAlbum.AlbumID,
                                                    Description = housing.Title,
                                                    PhotoLink = housing.PhotoURLs[j]
                                                };
                                                newPhotos.Add(newPhoto);
                                            }
                                            db.Photos.InsertAllOnSubmit(newPhotos);
                                        }
                                        else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                        {
                                            int i = 0;
                                            for (; i < housing.PhotoURLs.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                            for (int j = i; j < oldPhotos.Count; ++j)
                                            {
                                                db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                            }
                                        }
                                        db.SubmitChanges();

                                        housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                        return housing;
                                    }
                                }
                            }
                            else    // oldAddress exists. oldHousing not exist. Create newHousing.
                            {
                                var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                      where gl.LocationID == oldAddress.LocationID
                                                      select gl).SingleOrDefault();
                                //if (oldGeoLocation != null)
                                //{
                                //    oldGeoLocation.Latitude = housing.Latitude;
                                //    oldGeoLocation.Longitude = housing.Longitude;
                                //    db.SubmitChanges();
                                //}
                                //else
                                if (oldGeoLocation == null)
                                {
                                    GeoLocation newGeoLocation = new GeoLocation
                                    {
                                        Latitude = housing.Latitude,
                                        Longitude = housing.Longitude
                                    };
                                    db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                    db.SubmitChanges();

                                    oldAddress.LocationID = newGeoLocation.LocationID;
                                    db.SubmitChanges();
                                }

                                Housing newHousing = new Housing
                                {
                                    Title = housing.Title,
                                    OwnerID = currentUser.UserID,
                                    Price = housing.Price,
                                    IsAvailable = housing.IsAvailable,
                                    IsExist = true,
                                    HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                   where ht.HousingType == housing.HouseType
                                                   select ht.HouseTypeID).SingleOrDefault(),
                                    DateTimeCreated = DateTimeOffset.UtcNow,
                                    DateTimeExpired = null,
                                    NumOfView = 0,
                                    NumOfSaved = 0,
                                    NumOfPeople = housing.NumOfPeople,
                                    NumOfRoom = housing.NumOfRoom,
                                    NumOfBed = housing.NumOfBed,
                                    NumOfBath = housing.NumOfBath,
                                    AllowPet = housing.AllowPet,
                                    HasWifi = housing.HasWifi,
                                    HasAC = housing.HasAC,
                                    HasParking = housing.HasParking,
                                    TimeRestriction = housing.TimeRestriction,
                                    Area = housing.Area,

                                    AddressID = oldAddress.AddressID,

                                    Description = housing.Description,
                                    //LatestCommentID = null,
                                    NumOfComment = 0
                                };
                                db.Housings.InsertOnSubmit(newHousing);
                                db.SubmitChanges();

                                Album newAlbum = new Album
                                {
                                    CreatorID = currentUser.UserID,
                                    HousingID = newHousing.HousingID,
                                    AlbumName = housing.Title,
                                    DateTimeCreated = DateTimeOffset.UtcNow
                                };
                                db.Albums.InsertOnSubmit(newAlbum);
                                db.SubmitChanges();

                                // US culture.
                                var usCulture = new CultureInfo("en-US");
                                List<Photo> newPhotos = new List<Photo>();
                                for (int i = 0; i < housing.PhotoURLs.Count; ++i)
                                {
                                    String photoName = "IMG_" + currentUser.UID + "_"
                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                    if (i == 0)
                                    {
                                        photoName += "Profile";
                                    }
                                    else
                                    {
                                        photoName += i + "";
                                    }
                                    Photo newPhoto = new Photo
                                    {
                                        PhotoName = photoName,
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        AlbumID = newAlbum.AlbumID,
                                        Description = housing.Title,
                                        PhotoLink = housing.PhotoURLs[i]
                                    };
                                    newPhotos.Add(newPhoto);
                                }
                                db.Photos.InsertAllOnSubmit(newPhotos);
                                db.SubmitChanges();

                                housing.ID = newHousing.HousingID;
                                housing.Owner.UserID = newHousing.OwnerID;
                                housing.DateTimeCreated = newHousing.DateTimeCreated;

                                return housing;
                            }
                        }
                        else    // oldAddress not exist. Create new Address.
                        {
                            var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                  where (gl.Latitude == housing.Latitude)
                                                     && (gl.Longitude == housing.Longitude)
                                                  select gl).SingleOrDefault();
                            if (oldGeoLocation != null)
                            {
                                Address newAddress = new Address
                                {
                                    HouseNumber = housing.AddressHouseNumber,
                                    Street = housing.AddressStreet,
                                    Ward = housing.AddressWard,
                                    District = housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = oldGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // newAddress inserted. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = housing.Title;
                                    oldHousing.Price = housing.Price;
                                    oldHousing.IsAvailable = housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = housing.NumOfPeople;
                                    oldHousing.NumOfRoom = housing.NumOfRoom;
                                    oldHousing.NumOfBed = housing.NumOfBed;
                                    oldHousing.NumOfBath = housing.NumOfBath;
                                    oldHousing.AllowPet = housing.AllowPet;
                                    oldHousing.HasWifi = housing.HasWifi;
                                    oldHousing.HasAC = housing.HasAC;
                                    oldHousing.HasParking = housing.HasParking;
                                    oldHousing.TimeRestriction = housing.TimeRestriction;
                                    oldHousing.Area = housing.Area;

                                    oldHousing.AddressID = newAddress.AddressID;

                                    oldHousing.Description = housing.Description;
                                    //oldHousing.LatestCommentID = null;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture.
                                            var usCulture = new CultureInfo("en-US");
                                            if (housing.PhotoURLs.Count == oldPhotos.Count)
                                            {
                                                for (int i = 0; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = housing.Title,
                                                        PhotoLink = housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < housing.PhotoURLs.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                            return housing;
                                        }
                                    }
                                }
                                else    // newAddress inserted. oldHousing not exist. Create newHousing.
                                {
                                    Housing newHousing = new Housing
                                    {
                                        Title = housing.Title,
                                        OwnerID = currentUser.UserID,
                                        Price = housing.Price,
                                        IsAvailable = housing.IsAvailable,
                                        IsExist = true,
                                        HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                       where ht.HousingType == housing.HouseType
                                                       select ht.HouseTypeID).SingleOrDefault(),
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        NumOfPeople = housing.NumOfPeople,
                                        NumOfRoom = housing.NumOfRoom,
                                        NumOfBed = housing.NumOfBed,
                                        NumOfBath = housing.NumOfBath,
                                        AllowPet = housing.AllowPet,
                                        HasWifi = housing.HasWifi,
                                        HasAC = housing.HasAC,
                                        HasParking = housing.HasParking,
                                        TimeRestriction = housing.TimeRestriction,
                                        Area = housing.Area,

                                        AddressID = newAddress.AddressID,

                                        Description = housing.Description,
                                        //LatestCommentID = null,
                                        NumOfComment = 0
                                    };
                                    db.Housings.InsertOnSubmit(newHousing);
                                    db.SubmitChanges();

                                    Album newAlbum = new Album
                                    {
                                        CreatorID = currentUser.UserID,
                                        HousingID = newHousing.HousingID,
                                        AlbumName = housing.Title,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    };
                                    db.Albums.InsertOnSubmit(newAlbum);
                                    db.SubmitChanges();

                                    // US culture.
                                    var usCulture = new CultureInfo("en-US");
                                    List<Photo> newPhotos = new List<Photo>();
                                    for (int i = 0; i < housing.PhotoURLs.Count; ++i)
                                    {
                                        String photoName = "IMG_" + currentUser.UID + "_"
                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                        if (i == 0)
                                        {
                                            photoName += "Profile";
                                        }
                                        else
                                        {
                                            photoName += i + "";
                                        }
                                        Photo newPhoto = new Photo
                                        {
                                            PhotoName = photoName,
                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                            AlbumID = newAlbum.AlbumID,
                                            Description = housing.Title,
                                            PhotoLink = housing.PhotoURLs[i]
                                        };
                                        newPhotos.Add(newPhoto);
                                    }
                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                    db.SubmitChanges();

                                    housing.ID = newHousing.HousingID;
                                    housing.Owner.UserID = newHousing.OwnerID;
                                    housing.DateTimeCreated = newHousing.DateTimeCreated;

                                    return housing;
                                }
                            }
                            else    // oldGeoLocation not exist. Create newGeoLocation.
                            {
                                GeoLocation newGeoLocation = new GeoLocation
                                {
                                    Latitude = housing.Latitude,
                                    Longitude = housing.Longitude
                                };
                                db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                db.SubmitChanges();

                                Address newAddress = new Address
                                {
                                    HouseNumber = housing.AddressHouseNumber,
                                    Street = housing.AddressStreet,
                                    Ward = housing.AddressWard,
                                    District = housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = newGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // newAddress inserted. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = housing.Title;
                                    oldHousing.Price = housing.Price;
                                    oldHousing.IsAvailable = housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = housing.NumOfPeople;
                                    oldHousing.NumOfRoom = housing.NumOfRoom;
                                    oldHousing.NumOfBed = housing.NumOfBed;
                                    oldHousing.NumOfBath = housing.NumOfBath;
                                    oldHousing.AllowPet = housing.AllowPet;
                                    oldHousing.HasWifi = housing.HasWifi;
                                    oldHousing.HasAC = housing.HasAC;
                                    oldHousing.HasParking = housing.HasParking;
                                    oldHousing.TimeRestriction = housing.TimeRestriction;
                                    oldHousing.Area = housing.Area;

                                    oldHousing.AddressID = newAddress.AddressID;

                                    oldHousing.Description = housing.Description;
                                    //oldHousing.LatestCommentID = null;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture.
                                            var usCulture = new CultureInfo("en-US");
                                            if (housing.PhotoURLs.Count == oldPhotos.Count)
                                            {
                                                for (int i = 0; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = housing.Title,
                                                        PhotoLink = housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < housing.PhotoURLs.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                            return housing;
                                        }
                                    }
                                }
                                else    // newAddress inserted. oldHousing not exist. Create newHousing.
                                {
                                    Housing newHousing = new Housing
                                    {
                                        Title = housing.Title,
                                        OwnerID = currentUser.UserID,
                                        Price = housing.Price,
                                        IsAvailable = housing.IsAvailable,
                                        IsExist = true,
                                        HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                       where ht.HousingType == housing.HouseType
                                                       select ht.HouseTypeID).SingleOrDefault(),
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        NumOfPeople = housing.NumOfPeople,
                                        NumOfRoom = housing.NumOfRoom,
                                        NumOfBed = housing.NumOfBed,
                                        NumOfBath = housing.NumOfBath,
                                        AllowPet = housing.AllowPet,
                                        HasWifi = housing.HasWifi,
                                        HasAC = housing.HasAC,
                                        HasParking = housing.HasParking,
                                        TimeRestriction = housing.TimeRestriction,
                                        Area = housing.Area,

                                        AddressID = newAddress.AddressID,

                                        Description = housing.Description,
                                        //LatestCommentID = null,
                                        NumOfComment = 0
                                    };
                                    db.Housings.InsertOnSubmit(newHousing);
                                    db.SubmitChanges();

                                    Album newAlbum = new Album
                                    {
                                        CreatorID = currentUser.UserID,
                                        HousingID = newHousing.HousingID,
                                        AlbumName = housing.Title,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    };
                                    db.Albums.InsertOnSubmit(newAlbum);
                                    db.SubmitChanges();

                                    // US culture.
                                    var usCulture = new CultureInfo("en-US");
                                    List<Photo> newPhotos = new List<Photo>();
                                    for (int i = 0; i < housing.PhotoURLs.Count; ++i)
                                    {
                                        String photoName = "IMG_" + currentUser.UID + "_"
                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                        if (i == 0)
                                        {
                                            photoName += "Profile";
                                        }
                                        else
                                        {
                                            photoName += i + "";
                                        }
                                        Photo newPhoto = new Photo
                                        {
                                            PhotoName = photoName,
                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                            AlbumID = newAlbum.AlbumID,
                                            Description = housing.Title,
                                            PhotoLink = housing.PhotoURLs[i]
                                        };
                                        newPhotos.Add(newPhoto);
                                    }
                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                    db.SubmitChanges();

                                    housing.ID = newHousing.HousingID;
                                    housing.Owner.UserID = newHousing.OwnerID;
                                    housing.DateTimeCreated = newHousing.DateTimeCreated;

                                    return housing;
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

        [Route("")]
        [HttpPut]
        [Authorize]
        public bool UpdateHousing(HousingModel housing)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var city = (from c in db.GetTable<City>()
                                where c.CityName == housing.AddressCity
                                select c).SingleOrDefault();
                    if (city != null)
                    {
                        var oldAddress = (from a in db.GetTable<Address>()
                                          where (object.Equals(a.HouseNumber, housing.AddressHouseNumber))
                                             && (object.Equals(a.Street, housing.AddressStreet))
                                             && (object.Equals(a.Ward, housing.AddressWard))
                                             && (a.District == housing.AddressDistrict)
                                             && (a.CityID == city.CityID)
                                          select a).SingleOrDefault();

                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where (h.HousingID == housing.ID)
                                             && (h.OwnerID == currentUser.UserID)
                                          select h).SingleOrDefault();

                        if (oldAddress != null)
                        {
                            if (oldHousing != null)
                            {
                                var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                      where gl.LocationID == oldAddress.LocationID
                                                      select gl).SingleOrDefault();
                                //if (oldGeoLocation != null)
                                //{
                                //    oldGeoLocation.Latitude = housing.Latitude;
                                //    oldGeoLocation.Longitude = housing.Longitude;
                                //    db.SubmitChanges();
                                //}
                                //else
                                if (oldGeoLocation == null)
                                {
                                    GeoLocation newGeoLocation = new GeoLocation
                                    {
                                        Latitude = housing.Latitude,
                                        Longitude = housing.Longitude
                                    };
                                    db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                    db.SubmitChanges();

                                    oldAddress.LocationID = newGeoLocation.LocationID;
                                    db.SubmitChanges();
                                }

                                // oldAddress exists. oldHousing exists. Update existent Housing.
                                oldHousing.Title = housing.Title;
                                oldHousing.Price = housing.Price;
                                oldHousing.IsAvailable = housing.IsAvailable;
                                oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                          where ht.HousingType == housing.HouseType
                                                          select ht.HouseTypeID).SingleOrDefault();
                                oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                oldHousing.DateTimeExpired = null;
                                oldHousing.NumOfPeople = housing.NumOfPeople;
                                oldHousing.NumOfRoom = housing.NumOfRoom;
                                oldHousing.NumOfBed = housing.NumOfBed;
                                oldHousing.NumOfBath = housing.NumOfBath;
                                oldHousing.AllowPet = housing.AllowPet;
                                oldHousing.HasWifi = housing.HasWifi;
                                oldHousing.HasAC = housing.HasAC;
                                oldHousing.HasParking = housing.HasParking;
                                oldHousing.TimeRestriction = housing.TimeRestriction;
                                oldHousing.Area = housing.Area;

                                oldHousing.AddressID = oldAddress.AddressID;

                                oldHousing.Description = housing.Description;

                                // Update existent Album.
                                var oldAlbum = (from a in db.GetTable<Album>()
                                                where (a.HousingID == oldHousing.HousingID)
                                                   && (a.CreatorID == currentUser.UserID)
                                                select a).SingleOrDefault();
                                if (oldAlbum != null)
                                {
                                    oldAlbum.AlbumName = housing.Title;
                                    oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                    // Update existent Photos.
                                    var oldPhotos = (from p in db.GetTable<Photo>()
                                                     where p.AlbumID == oldAlbum.AlbumID
                                                     orderby p.PhotoID
                                                     select p).ToList();
                                    if (oldPhotos != null && oldPhotos.Count > 0)
                                    {
                                        // US culture.
                                        var usCulture = new CultureInfo("en-US");
                                        if (housing.PhotoURLs.Count == oldPhotos.Count)
                                        {
                                            for (int i = 0; i < oldPhotos.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                        }
                                        else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                        {
                                            int i = 0;
                                            for (; i < oldPhotos.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                            List<Photo> newPhotos = new List<Photo>();
                                            for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                Photo newPhoto = new Photo
                                                {
                                                    PhotoName = photoName,
                                                    DateTimeCreated = DateTimeOffset.UtcNow,
                                                    AlbumID = oldAlbum.AlbumID,
                                                    Description = housing.Title,
                                                    PhotoLink = housing.PhotoURLs[j]
                                                };
                                                newPhotos.Add(newPhoto);
                                            }
                                            db.Photos.InsertAllOnSubmit(newPhotos);
                                        }
                                        else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                        {
                                            int i = 0;
                                            for (; i < housing.PhotoURLs.Count; ++i)
                                            {
                                                String photoName = "IMG_" + currentUser.UID + "_"
                                                    + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                if (i == 0)
                                                {
                                                    photoName += "Profile";
                                                }
                                                else
                                                {
                                                    photoName += i + "";
                                                }
                                                oldPhotos[i].PhotoName = photoName;
                                                oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldPhotos[i].Description = housing.Title;
                                                oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                            }
                                            for (int j = i; j < oldPhotos.Count; ++j)
                                            {
                                                db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                            }
                                        }
                                        db.SubmitChanges();

                                        housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                        return true;
                                    }
                                }
                            }
                        }
                        else    // oldAddress not exist. Create new Address.
                        {
                            var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                  where (gl.Latitude == housing.Latitude)
                                                     && (gl.Longitude == housing.Longitude)
                                                  select gl).SingleOrDefault();
                            if (oldGeoLocation != null)
                            {
                                Address newAddress = new Address
                                {
                                    HouseNumber = housing.AddressHouseNumber,
                                    Street = housing.AddressStreet,
                                    Ward = housing.AddressWard,
                                    District = housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = oldGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // newAddress inserted. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = housing.Title;
                                    oldHousing.Price = housing.Price;
                                    oldHousing.IsAvailable = housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = housing.NumOfPeople;
                                    oldHousing.NumOfRoom = housing.NumOfRoom;
                                    oldHousing.NumOfBed = housing.NumOfBed;
                                    oldHousing.NumOfBath = housing.NumOfBath;
                                    oldHousing.AllowPet = housing.AllowPet;
                                    oldHousing.HasWifi = housing.HasWifi;
                                    oldHousing.HasAC = housing.HasAC;
                                    oldHousing.HasParking = housing.HasParking;
                                    oldHousing.TimeRestriction = housing.TimeRestriction;
                                    oldHousing.Area = housing.Area;

                                    oldHousing.AddressID = newAddress.AddressID;

                                    oldHousing.Description = housing.Description;
                                    //oldHousing.LatestCommentID = null;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture.
                                            var usCulture = new CultureInfo("en-US");
                                            if (housing.PhotoURLs.Count == oldPhotos.Count)
                                            {
                                                for (int i = 0; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = housing.Title,
                                                        PhotoLink = housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < housing.PhotoURLs.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                            return true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GeoLocation newGeoLocation = new GeoLocation
                                {
                                    Latitude = housing.Latitude,
                                    Longitude = housing.Longitude
                                };
                                db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                db.SubmitChanges();

                                Address newAddress = new Address
                                {
                                    HouseNumber = housing.AddressHouseNumber,
                                    Street = housing.AddressStreet,
                                    Ward = housing.AddressWard,
                                    District = housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = newGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // newAddress inserted. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = housing.Title;
                                    oldHousing.Price = housing.Price;
                                    oldHousing.IsAvailable = housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = housing.NumOfPeople;
                                    oldHousing.NumOfRoom = housing.NumOfRoom;
                                    oldHousing.NumOfBed = housing.NumOfBed;
                                    oldHousing.NumOfBath = housing.NumOfBath;
                                    oldHousing.AllowPet = housing.AllowPet;
                                    oldHousing.HasWifi = housing.HasWifi;
                                    oldHousing.HasAC = housing.HasAC;
                                    oldHousing.HasParking = housing.HasParking;
                                    oldHousing.TimeRestriction = housing.TimeRestriction;
                                    oldHousing.Area = housing.Area;

                                    oldHousing.AddressID = newAddress.AddressID;

                                    oldHousing.Description = housing.Description;
                                    //oldHousing.LatestCommentID = null;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture.
                                            var usCulture = new CultureInfo("en-US");
                                            if (housing.PhotoURLs.Count == oldPhotos.Count)
                                            {
                                                for (int i = 0; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (housing.PhotoURLs.Count > oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < oldPhotos.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = housing.Title,
                                                        PhotoLink = housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < housing.PhotoURLs.Count; ++i)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_";
                                                    if (i == 0)
                                                    {
                                                        photoName += "Profile";
                                                    }
                                                    else
                                                    {
                                                        photoName += i + "";
                                                    }
                                                    oldPhotos[i].PhotoName = photoName;
                                                    oldPhotos[i].DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldPhotos[i].Description = housing.Title;
                                                    oldPhotos[i].PhotoLink = housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            housing.DateTimeCreated = oldHousing.DateTimeCreated;
                                            return true;
                                        }
                                    }
                                }
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

        [Route("")]
        [HttpDelete]
        [Authorize]
        public bool DeleteHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldHousing = (from h in db.GetTable<Housing>()
                                      where h.HousingID == housingID
                                      select h).SingleOrDefault();
                    // User created both Share and Housing. Delete both.
                    if (oldHousing != null)
                    {
                        var oldAlbum = (from a in db.GetTable<Album>()
                                        where (a.HousingID == oldHousing.HousingID)
                                           && (a.CreatorID == currentUser.UserID)
                                        select a).SingleOrDefault();
                        if (oldAlbum != null)
                        {
                            var oldPhotos = (from p in db.GetTable<Photo>()
                                             where p.AlbumID == oldAlbum.AlbumID
                                             orderby p.PhotoID
                                             select p).ToList();
                            if (oldPhotos != null && oldPhotos.Count > 0)
                            {
                                db.Photos.DeleteAllOnSubmit(oldPhotos);
                                db.SubmitChanges();

                                db.Albums.DeleteOnSubmit(oldAlbum);
                                db.SubmitChanges();

                                // Delete all user's Share posts of current Housing.
                                var oldShareHousings = (from s in db.GetTable<ShareHousing>()
                                                        where s.HousingID == oldHousing.HousingID
                                                        select s).ToList();
                                if (oldShareHousings != null && oldShareHousings.Count > 0)
                                {
                                    foreach (var item in oldShareHousings)
                                    {
                                        // Delete all user's saved share housings.
                                        var oldSavedShareHousings = (from ss in db.GetTable<SavedShareHousing>()
                                                                    where ss.ShareHousingID == item.ShareHousingID
                                                                    select ss).ToList();
                                        if (oldSavedShareHousings != null && oldSavedShareHousings.Count > 0)
                                        {
                                            db.SavedShareHousings.DeleteAllOnSubmit(oldSavedShareHousings);
                                            db.SubmitChanges();
                                        }

                                        // Delete all user's appointment of current Share Housing.
                                        var oldShareHousingAppointments = (from a in db.GetTable<ShareHousingAppointment>()
                                                                          where a.ShareHousingID == item.ShareHousingID
                                                                          select a).ToList();
                                        if (oldShareHousingAppointments != null && oldShareHousingAppointments.Count > 0)
                                        {
                                            db.ShareHousingAppointments.DeleteAllOnSubmit(oldShareHousingAppointments);
                                            db.SubmitChanges();
                                        }
                                    }
                                    db.ShareHousings.DeleteAllOnSubmit(oldShareHousings);
                                    db.SubmitChanges();
                                }

                                // Delete all user's saved housings.
                                var oldSavedHousings = (from s in db.GetTable<SavedHousing>()
                                                       where s.HousingID == oldHousing.HousingID
                                                       select s).ToList();
                                if (oldSavedHousings != null && oldSavedHousings.Count > 0)
                                {
                                    db.SavedHousings.DeleteAllOnSubmit(oldSavedHousings);
                                    db.SubmitChanges();
                                }

                                // Delete all user's notes of current Housing.
                                var oldNotes = (from n in db.GetTable<Note>()
                                                where n.HousingID == oldHousing.HousingID
                                                select n).ToList();
                                if (oldNotes != null && oldNotes.Count > 0)
                                {
                                    db.Notes.DeleteAllOnSubmit(oldNotes);
                                    db.SubmitChanges();
                                }

                                // Delete all user's appointment of current Housing.
                                var oldAppointments = (from a in db.GetTable<HousingAppointment>()
                                                      where a.HousingID == oldHousing.HousingID
                                                      select a).ToList();
                                if (oldAppointments != null && oldAppointments.Count > 0)
                                {
                                    db.HousingAppointments.DeleteAllOnSubmit(oldAppointments);
                                    db.SubmitChanges();
                                }

                                // Delete all user's comment of current Housing.
                                var oldComments = (from c in db.GetTable<Comment>()
                                                   where c.HousingID == oldHousing.HousingID
                                                   select c).ToList();
                                if (oldComments != null && oldComments.Count > 0)
                                {
                                    db.Comments.DeleteAllOnSubmit(oldComments);
                                    db.SubmitChanges();
                                }

                                db.Housings.DeleteOnSubmit(oldHousing);
                                db.SubmitChanges();

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

        [Route("report")]
        [HttpPost]
        [Authorize]
        public bool ReportHousing(int housingID)
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
                        if (currentUser.UserID != currentHousing.OwnerID)
                        {
                            var currentShareHousings = (from sh in db.GetTable<ShareHousing>()
                                                        where sh.HousingID == currentHousing.HousingID
                                                        select sh).ToList();
                            if (currentShareHousings != null && currentShareHousings.Count > 0)
                            {
                                foreach (var shareHousing in currentShareHousings)
                                {
                                    shareHousing.DateTimeExpired = DateTimeOffset.UtcNow.AddDays(3);
                                }
                            }
                            currentHousing.DateTimeExpired = DateTimeOffset.UtcNow.AddDays(3);    // This housing will be expired in next 3 days.
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

        [Route("note")]
        [HttpGet]
        [Authorize]
        public NoteModel GetCurrentUserNote(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldNote = (from n in db.GetTable<Note>()
                                   where (n.HousingID == housingID)
                                      && (n.CreatorID == currentUser.UserID)
                                   select n).SingleOrDefault();
                    if (oldNote != null)
                    {
                        return new NoteModel
                        {
                            HousingID = oldNote.HousingID,
                            NoteName = oldNote.NoteName,
                            Content = oldNote.Content,
                            DateTimeCreated = oldNote.DateTimeCreated
                        };
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

        [Route("note")]
        [HttpPost]
        [Authorize]
        public NoteModel PostNote(NoteModel note, int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldNote = (from n in db.GetTable<Note>()
                                   where (n.HousingID == note.HousingID)
                                      && (n.CreatorID == currentUser.UserID)
                                   select n).SingleOrDefault();
                    if (oldNote != null)
                    {
                        oldNote.NoteName = note.NoteName;
                        oldNote.Content = note.Content;
                        oldNote.DateTimeCreated = DateTimeOffset.UtcNow;

                        db.SubmitChanges();

                        note.DateTimeCreated = oldNote.DateTimeCreated;

                        return note;
                    }
                    else
                    {
                        Note newNote = new Note
                        {
                            CreatorID = currentUser.UserID,
                            HousingID = note.HousingID,
                            //NoteName = (from h in db.GetTable<Housing>()
                            //            where h.HousingID == note.HousingID
                            //            select h.Title).SingleOrDefault(),
                            ShareHousingID = shareHousingID,
                            NoteName = note.NoteName,
                            Content = note.Content,
                            DateTimeCreated = DateTimeOffset.UtcNow
                        };
                        db.Notes.InsertOnSubmit(newNote);
                        db.SubmitChanges();

                        note.NoteName = newNote.NoteName;
                        note.DateTimeCreated = newNote.DateTimeCreated;

                        return note;
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

        [Route("note")]
        [HttpPut]
        [Authorize]
        public NoteModel UpdateNote(NoteModel note)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldNote = (from n in db.GetTable<Note>()
                                   where (n.HousingID == note.HousingID)
                                      && (n.CreatorID == currentUser.UserID)
                                   select n).SingleOrDefault();
                    if (oldNote != null)
                    {
                        oldNote.NoteName = note.NoteName;
                        oldNote.Content = note.Content;
                        oldNote.DateTimeCreated = DateTimeOffset.UtcNow;

                        db.SubmitChanges();

                        note.DateTimeCreated = oldNote.DateTimeCreated;

                        return note;
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

        [Route("note")]
        [HttpDelete]
        [Authorize]
        public bool DeleteNote(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldNote = (from n in db.GetTable<Note>()
                                   where (n.HousingID == housingID)
                                      && (n.CreatorID == currentUser.UserID)
                                   select n).SingleOrDefault();
                    if (oldNote != null)
                    {
                        db.Notes.DeleteOnSubmit(oldNote);
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

        [Route("photo")]
        [HttpGet]
        [Authorize]
        public List<String> GetCurrentUserPhotoURLs(int housingID)
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
                        var oldAlbum = (from a in db.GetTable<Album>()
                                        where (a.HousingID == currentHousing.HousingID)
                                           && (a.CreatorID == currentUser.UserID)
                                        select a).SingleOrDefault();
                        if (oldAlbum != null)
                        {
                            return (from p in db.GetTable<Photo>()
                                    where p.AlbumID == oldAlbum.AlbumID
                                    orderby p.PhotoID
                                    select p.PhotoLink).ToList();
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

        [Route("photo")]
        [HttpPost]
        [Authorize]
        public bool PostPhotoURL(int housingID, String photoURL, int shareHousingID)
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
                        var oldAlbum = (from a in db.GetTable<Album>()
                                        where (a.HousingID == housingID)
                                           && (a.CreatorID == currentUser.UserID)
                                        select a).SingleOrDefault();
                        if (oldAlbum != null)
                        {
                            // US culture.
                            var usCulture = new CultureInfo("en-US");
                            String photoName = "IMG_" + currentUser.UID + "_"
                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture);
                            Photo newPhoto = new Photo
                            {
                                PhotoName = photoName,
                                DateTimeCreated = DateTimeOffset.UtcNow,
                                AlbumID = oldAlbum.AlbumID,
                                Description = currentHousing.Title,
                                PhotoLink = photoURL
                            };
                            db.Photos.InsertOnSubmit(newPhoto);
                            db.SubmitChanges();
                            return true;
                        }
                        else
                        {
                            Album newAlbum = new Album
                            {
                                CreatorID = currentUser.UserID,
                                HousingID = housingID,
                                ShareHousingID = shareHousingID,
                                AlbumName = (from h in db.GetTable<Housing>()
                                             where (h.HousingID == housingID)
                                             select h.Title).SingleOrDefault(),
                                DateTimeCreated = DateTimeOffset.UtcNow
                            };
                            db.Albums.InsertOnSubmit(newAlbum);
                            db.SubmitChanges();

                            // US culture.
                            var usCulture = new CultureInfo("en-US");
                            String photoName = "IMG_" + currentUser.UID + "_"
                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture);
                            Photo newPhoto = new Photo
                            {
                                PhotoName = photoName,
                                DateTimeCreated = DateTimeOffset.UtcNow,
                                AlbumID = newAlbum.AlbumID,
                                Description = currentHousing.Title,
                                PhotoLink = photoURL
                            };
                            db.Photos.InsertOnSubmit(newPhoto);
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

        [Route("photo")]
        [HttpPut]
        [Authorize]
        public bool UpdatePhotoURL(int housingID, String photoURL, int index)
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
                        var oldAlbum = (from a in db.GetTable<Album>()
                                        where (a.HousingID == housingID)
                                           && (a.CreatorID == currentUser.UserID)
                                        select a).SingleOrDefault();
                        if (oldAlbum != null)
                        {
                            var oldPhotos = (from p in db.GetTable<Photo>()
                                            where p.AlbumID == oldAlbum.AlbumID
                                            orderby p.PhotoID
                                            select p).ToList();
                            if (oldPhotos != null && oldPhotos.Count > 0
                                && index >= 0 && index < oldPhotos.Count)
                            {
                                oldPhotos[index].PhotoLink = photoURL;
                                db.SubmitChanges();
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

        [Route("photo")]
        [HttpDelete]
        [Authorize]
        public bool DeletePhotoURL(int housingID, int index)
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
                        var oldAlbum = (from a in db.GetTable<Album>()
                                        where (a.HousingID == housingID)
                                           && (a.CreatorID == currentUser.UserID)
                                        select a).SingleOrDefault();
                        if (oldAlbum != null)
                        {
                            var oldPhotos = (from p in db.GetTable<Photo>()
                                            where p.AlbumID == oldAlbum.AlbumID
                                            orderby p.PhotoID
                                            select p).ToList();
                            if (oldPhotos != null && oldPhotos.Count > 0
                                && index >= 0 && index < oldPhotos.Count)
                            {
                                if (oldPhotos.Count == 1)
                                {
                                    db.Photos.DeleteOnSubmit(oldPhotos[index]);
                                    db.SubmitChanges();
                                    db.Albums.DeleteOnSubmit(oldAlbum);
                                    db.SubmitChanges();
                                }
                                else if (oldPhotos.Count > 1)
                                {
                                    db.Photos.DeleteOnSubmit(oldPhotos[index]);
                                    db.SubmitChanges();
                                }
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
    }
}
