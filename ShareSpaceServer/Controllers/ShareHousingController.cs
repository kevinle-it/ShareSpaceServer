using ShareSpaceServer.DBMapping;
using ShareSpaceServer.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShareSpaceServer.Controllers
{
    [RoutePrefix("api/sharehousing")]
    public class ShareHousingController : BaseController
    {
        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        public List<ShareHousingModel> GetMoreOlderShareHousings(string currentBottomShareHousingDateTimeCreated = null)   //, int offset)
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                List<ShareHousingModel> olderShareHousings = new List<ShareHousingModel>();

                DateTimeOffset? parsedDate = base.ParseDateFromStringOrNull(currentBottomShareHousingDateTimeCreated);
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
                         where (sh.DateTimeCreated < parsedDate)
                            && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                            && (h.IsAvailable == true)
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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
                             DateTimeCreated = sh.DateTimeCreated
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
                         where ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                            && (h.IsAvailable == true)
                            && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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
                             DateTimeCreated = sh.DateTimeCreated
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpGet]
        public List<ShareHousingModel> GetMoreNewerShareHousings(string currentTopShareHousingDateTimeCreated)
        {
            try
            {
                DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                DateTimeOffset parsedDate = base.ParseDateFromString(currentTopShareHousingDateTimeCreated);

                List<ShareHousingModel> newerShareHousings =
                    (from sh in db.GetTable<ShareHousing>()
                     join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                     join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                     join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                     join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                     join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                     join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                     join c in db.GetTable<City>() on a.CityID equals c.CityID
                     //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                     where (sh.DateTimeCreated > parsedDate)
                        && ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                        && (h.IsAvailable == true)
                        && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
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
                         DateTimeCreated = sh.DateTimeCreated
                     }).Take(5).ToList();

                if (newerShareHousings != null & newerShareHousings.Count > 0)
                {
                    foreach (var item in newerShareHousings)
                    {
                        item.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                  join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                  where (a.HousingID == item.Housing.ID)
                                                     && (a.CreatorID == item.Housing.Owner.UserID)
                                                  orderby p.PhotoID
                                                  select p.PhotoLink).ToList();
                    }
                }                
                return newerShareHousings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        [Route("search")] // <- This is still required for Web API 2 to work. Otherwise it won't find the matched method to execute although there's no error.
        [HttpPost]
        public List<ShareHousingModel> SearchShareHousings(SearchShareHousingDataModel data)
        {
            try
            {
                // Price Criteria.
                int[] pricePerMonthOfOneRange = new int[2];
                if (data.MinPricePerMonthOfOne == -2)    // Any.
                {
                    if (data.MaxPricePerMonthOfOne == -2)    // Any.
                    {
                        pricePerMonthOfOneRange[0] = -2;
                        pricePerMonthOfOneRange[1] = -2;
                    }
                    else if (data.MaxPricePerMonthOfOne == -1)   // Deal => No such case.
                    {
                        return null;
                    }
                    else    // Other Max Prices.
                    {
                        pricePerMonthOfOneRange[0] = 0;
                        pricePerMonthOfOneRange[1] = data.MaxPricePerMonthOfOne;
                    }
                }
                else if (data.MinPricePerMonthOfOne == -1)   // Deal.
                {
                    if (data.MaxPricePerMonthOfOne == -1)    // Deal.
                    {
                        pricePerMonthOfOneRange[0] = -1;
                        pricePerMonthOfOneRange[1] = -1;
                    }
                    else    // Not Deal => No such case.
                    {
                        return null;
                    }
                }
                else    // Other Min Prices.
                {
                    if (data.MaxPricePerMonthOfOne == -2)    // Any.
                    {
                        pricePerMonthOfOneRange[0] = data.MinPricePerMonthOfOne;
                        pricePerMonthOfOneRange[1] = 1000000000;
                    }
                    else if (data.MaxPricePerMonthOfOne == -1)   // Deal => No such case.
                    {
                        return null;
                    }
                    else    // Other Max Prices.
                    {
                        pricePerMonthOfOneRange[0] = data.MinPricePerMonthOfOne;
                        pricePerMonthOfOneRange[1] = data.MaxPricePerMonthOfOne;
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

                // Gender Criteria.
                int requiredGenderID = (from g in db.GetTable<Gender>()
                                        where g.GenderType == data.RequiredGender
                                        select g.GenderID).SingleOrDefault();

                // Work Type Criteria.
                int requiredWorkTypeID = (from w in db.GetTable<Work>()
                                          where w.WorkType == data.RequiredWorkType
                                          select w.WorkID).SingleOrDefault();

                var resultShareHousingsQuery =
                    (from sh in db.GetTable<ShareHousing>()
                     join h in db.GetTable<Housing>() on sh.HousingID equals h.HousingID
                     join u1 in db.GetTable<ShareSpaceUser>() on h.OwnerID equals u1.UserID
                     join u2 in db.GetTable<ShareSpaceUser>() on sh.CreatorID equals u2.UserID
                     join ht in db.GetTable<HouseType>() on h.HouseTypeID equals ht.HouseTypeID
                     join a in db.GetTable<Address>() on h.AddressID equals a.AddressID
                     join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                     join c in db.GetTable<City>() on a.CityID equals c.CityID
                     //join comment in db.GetTable<Comment>() on h.LatestCommentID equals comment.CommentID
                     where (((pricePerMonthOfOneRange[0] != -2 && pricePerMonthOfOneRange[1] != -2) && (pricePerMonthOfOneRange[0] != -1 && pricePerMonthOfOneRange[1] != -1))
                           ? (pricePerMonthOfOneRange[0] <= sh.PricePerMonthOfOne && sh.PricePerMonthOfOne <= pricePerMonthOfOneRange[1])
                           : ((pricePerMonthOfOneRange[0] == -1 && pricePerMonthOfOneRange[1] == -1) ? sh.PricePerMonthOfOne == -1 : -1 <= sh.PricePerMonthOfOne))
                        && (areaRange[0] <= h.Area && h.Area <= areaRange[1])
                        && (data.NumPeople != -1 ? data.NumPeople <= h.NumOfPeople : -1 <= h.NumOfPeople)
                        && (data.NumRoom != -1 ? data.NumRoom <= h.NumOfRoom : -1 <= h.NumOfRoom)
                        && (data.NumBed != -1 ? data.NumBed <= h.NumOfBed : -1 <= h.NumOfBed)
                        && (data.NumBath != -1 ? data.NumBath <= h.NumOfBath : -1 <= h.NumOfBath)
                        && ((timeRestrictionRange[0] == new TimeSpan(0, 0, 0) && timeRestrictionRange[1] == new TimeSpan(23, 59, 59))
                           ? true
                           : timeRestrictionRange[0] <= h.TimeRestriction && h.TimeRestriction <= timeRestrictionRange[1])
                        && (data.NumRoommate <= sh.RequiredNumOfPeople)
                        && (requiredGenderID == 0 ? true : sh.RequiredGenderID == requiredGenderID)
                        && (requiredWorkTypeID == 0 ? true : sh.RequiredWorkID == requiredWorkTypeID)
                        && 
                           ((sh.DateTimeExpired == null) || (DateTimeOffset.UtcNow < sh.DateTimeExpired))
                        && (h.IsAvailable == true)
                        && ((h.DateTimeExpired == null) || (DateTimeOffset.UtcNow < h.DateTimeExpired))   // DateTimeExpired == null <=> Forever
                     orderby sh.DateTimeCreated descending
                     //select new ShareHousingModel
                     //{
                     //    ID = sh.ShareHousingID,
                     //    Housing = new HousingModel
                     //    {
                     //        ID = h.HousingID,
                     //        Title = h.Title,
                     //        Owner = new UserModel
                     //        {
                     //            UserID = h.OwnerID,
                     //            UID = u1.UID,
                     //            FirstName = u1.FirstName,
                     //            LastName = u1.LastName,
                     //            Email = (from user in db.GetTable<ShareSpaceUser>()
                     //                     join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                     //                     join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                     //                     where user.UserID == h.OwnerID
                     //                     select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                     //            DOB = u1.DOB,
                     //            PhoneNumber = u1.PhoneNumber,
                     //            Gender = (from user in db.GetTable<ShareSpaceUser>()
                     //                      join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                     //                      where user.UserID == h.OwnerID
                     //                      select g.GenderType).SingleOrDefault(),
                     //            NumOfNote = u1.NumOfNote,
                     //            DeviceTokens = u1.DeviceTokens
                     //        },
                     //        Price = h.Price,
                     //        IsAvailable = h.IsAvailable,
                     //        HouseType = ht.HousingType,
                     //        DateTimeCreated = h.DateTimeCreated,
                     //        NumOfView = h.NumOfView,
                     //        NumOfSaved = h.NumOfSaved,
                     //        NumOfPeople = h.NumOfPeople,
                     //        NumOfRoom = h.NumOfRoom,
                     //        NumOfBed = h.NumOfBed,
                     //        NumOfBath = h.NumOfBath,
                     //        AllowPet = h.AllowPet,
                     //        HasWifi = h.HasWifi,
                     //        HasAC = h.HasAC,
                     //        HasParking = h.HasParking,
                     //        TimeRestriction = h.TimeRestriction,
                     //        Area = h.Area,
                     //        Latitude = gl.Latitude,
                     //        Longitude = gl.Longitude,
                     //        AddressHouseNumber = a.HouseNumber,
                     //        AddressStreet = a.Street,
                     //        AddressWard = a.Ward,
                     //        AddressDistrict = a.District,
                     //        AddressCity = c.CityName,
                     //        Description = h.Description,
                     //        NumOfComment = h.NumOfComment
                     //    },
                     //    Creator = new UserModel
                     //    {
                     //        UserID = sh.CreatorID,
                     //        FirstName = u2.FirstName,
                     //        LastName = u2.LastName,
                     //        Email = (from user in db.GetTable<ShareSpaceUser>()
                     //                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                     //                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                     //                 where user.UserID == sh.CreatorID
                     //                 select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                     //        DOB = u2.DOB,
                     //        PhoneNumber = u2.PhoneNumber,
                     //        Gender = (from user in db.GetTable<ShareSpaceUser>()
                     //                  join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                     //                  where user.UserID == sh.CreatorID
                     //                  select g.GenderType).SingleOrDefault(),
                     //        NumOfNote = u2.NumOfNote,
                     //        DeviceTokens = u2.DeviceTokens
                     //    },
                     //    IsAvailable = sh.IsAvailable,
                     //    PricePerMonthOfOne = sh.PricePerMonthOfOne,
                     //    Description = sh.Description,
                     //    NumOfView = sh.NumOfView,
                     //    NumOfSaved = sh.NumOfSaved,
                     //    RequiredNumOfPeople = sh.RequiredNumOfPeople,
                     //    RequiredGender = (from g in db.GetTable<Gender>()
                     //                      where g.GenderID == sh.RequiredGenderID
                     //                      select g.GenderType).SingleOrDefault(),
                     //    RequiredWorkType = (from w in db.GetTable<Work>()
                     //                        where w.WorkID == sh.RequiredWorkID
                     //                        select w.WorkType).SingleOrDefault(),
                     //    AllowSmoking = sh.AllowSmoking,
                     //    AllowAlcohol = sh.AllowAlcohol,
                     //    HasPrivateKey = sh.HasPrivateKey,
                     //    DateTimeCreated = sh.DateTimeCreated
                     //}
                     select new { sh, h, u1, u2, ht, a, gl, c });

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
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.h.AllowPet == amenities[0]);
                        }
                        else if (i == 1 && amenities[1])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.h.HasWifi == amenities[1]);
                        }
                        else if (i == 2 && amenities[2])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.h.HasAC == amenities[2]);
                        }
                        else if (i == 3 && amenities[3])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.h.HasParking == amenities[3]);
                        }
                    }
                }

                // Where Clause for Other Info Criteria.
                if (!data.OtherInfo[0])
                {
                    // Other Info Criteria.
                    bool[] otherInfo = new bool[3];
                    for (int i = 0; i < data.OtherInfo.Length; ++i)
                    {
                        if (i != 0)
                        {
                            otherInfo[i - 1] = data.OtherInfo[i];
                        }
                    }
                    for (int i = 0; i < otherInfo.Length; ++i)
                    {
                        if (i == 0 && otherInfo[0])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.sh.AllowSmoking == otherInfo[0]);
                        }
                        else if (i == 1 && otherInfo[1])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.sh.AllowAlcohol == otherInfo[1]);
                        }
                        else if (i == 2 && otherInfo[2])
                        {
                            resultShareHousingsQuery = resultShareHousingsQuery.Where(item => item.sh.HasPrivateKey == otherInfo[2]);
                        }
                    }
                }

                List<ShareHousingModel> resultShareHousings =
                    resultShareHousingsQuery.Select(
                        item => new ShareHousingModel
                        {
                            ID = item.sh.ShareHousingID,
                            Housing = new HousingModel
                            {
                                ID = item.h.HousingID,
                                Title = item.h.Title,
                                Owner = new UserModel
                                {
                                    UserID = item.h.OwnerID,
                                    UID = item.u1.UID,
                                    FirstName = item.u1.FirstName,
                                    LastName = item.u1.LastName,
                                    Email = (from user in db.GetTable<ShareSpaceUser>()
                                             join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                             where user.UserID == item.h.OwnerID
                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                    DOB = item.u1.DOB,
                                    PhoneNumber = item.u1.PhoneNumber,
                                    Gender = (from user in db.GetTable<ShareSpaceUser>()
                                              join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                              where user.UserID == item.h.OwnerID
                                              select g.GenderType).SingleOrDefault(),
                                    NumOfNote = item.u1.NumOfNote,
                                    DeviceTokens = item.u1.DeviceTokens
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
                                NumOfComment = item.h.NumOfComment
                            },
                            Creator = new UserModel
                            {
                                UserID = item.sh.CreatorID,
                                FirstName = item.u2.FirstName,
                                LastName = item.u2.LastName,
                                Email = (from user in db.GetTable<ShareSpaceUser>()
                                         join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                         join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                         where user.UserID == item.sh.CreatorID
                                         select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                DOB = item.u2.DOB,
                                PhoneNumber = item.u2.PhoneNumber,
                                Gender = (from user in db.GetTable<ShareSpaceUser>()
                                          join g in db.GetTable<Gender>() on user.GenderID equals g.GenderID
                                          where user.UserID == item.sh.CreatorID
                                          select g.GenderType).SingleOrDefault(),
                                NumOfNote = item.u2.NumOfNote,
                                DeviceTokens = item.u2.DeviceTokens
                            },
                            IsAvailable = item.sh.IsAvailable,
                            PricePerMonthOfOne = item.sh.PricePerMonthOfOne,
                            Description = item.sh.Description,
                            NumOfView = item.sh.NumOfView,
                            NumOfSaved = item.sh.NumOfSaved,
                            RequiredNumOfPeople = item.sh.RequiredNumOfPeople,
                            RequiredGender = (from g in db.GetTable<Gender>()
                                              where g.GenderID == item.sh.RequiredGenderID
                                              select g.GenderType).SingleOrDefault(),
                            RequiredWorkType = (from w in db.GetTable<Work>()
                                                where w.WorkID == item.sh.RequiredWorkID
                                                select w.WorkType).SingleOrDefault(),
                            AllowSmoking = item.sh.AllowSmoking,
                            AllowAlcohol = item.sh.AllowAlcohol,
                            HasPrivateKey = item.sh.HasPrivateKey,
                            DateTimeCreated = item.sh.DateTimeCreated
                        }).ToList();

                if (resultShareHousings != null & resultShareHousings.Count > 0)
                {
                    List<int> removeIndexes = new List<int>();

                    // Keywords Criteria.
                    if (!String.IsNullOrEmpty(data.Keywords))
                    {
                        var keywordsPunctuation = data.Keywords.ToLower().Where(Char.IsPunctuation).Distinct().ToArray();
                        string[] splitKeywords = data.Keywords.ToLower().Split().Select(x => x.Trim(keywordsPunctuation)).ToArray();

                        for (int i = 0; i < resultShareHousings.Count; ++i)
                        {
                            string title = resultShareHousings.ElementAt(i).Housing.Title.ToLower();
                            var titlePunctuation = title.Where(Char.IsPunctuation).Distinct().ToArray();
                            string[] titleWords = title.Split().Select(x => x.Trim(titlePunctuation)).ToArray();

                            string housingDescription = resultShareHousings.ElementAt(i).Housing.Description.ToLower();
                            var housingDescriptionPunctuation = housingDescription.Where(Char.IsPunctuation).Distinct().ToArray();
                            string[] housingDescriptionWords = housingDescription.Split().Select(x => x.Trim(housingDescriptionPunctuation)).ToArray();

                            string shareHousingDescription = resultShareHousings.ElementAt(i).Description.ToLower();
                            var shareHousingDescriptionPunctuation = shareHousingDescription.Where(Char.IsPunctuation).Distinct().ToArray();
                            string[] shareHousingDescriptionWords = shareHousingDescription.Split().Select(x => x.Trim(shareHousingDescriptionPunctuation)).ToArray();

                            if (!titleWords.Any(w => splitKeywords.Contains(w))
                                && !housingDescriptionWords.Any(w => splitKeywords.Contains(w))
                                && !shareHousingDescriptionWords.Any(w => splitKeywords.Contains(w)))
                            {
                                removeIndexes.Add(i);
                            }
                        }
                        foreach (var index in removeIndexes.OrderByDescending(item => item))
                        {
                            resultShareHousings.RemoveAt(index);
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
                            for (int i = 0; i < resultShareHousings.Count; ++i)
                            {
                                if (!houseTypes.Any(type => resultShareHousings.ElementAt(i).Housing.HouseType == type))
                                {
                                    removeIndexes.Add(i);
                                }
                            }
                            foreach (var index in removeIndexes.OrderByDescending(item => item))
                            {
                                resultShareHousings.RemoveAt(index);
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
                        for (int i = 0; i < resultShareHousings.Count; ++i)
                        {
                            GeoCoordinate currentHousingGeoCoordinate = new GeoCoordinate(
                                    (double) resultShareHousings.ElementAt(i).Housing.Latitude,
                                    (double) resultShareHousings.ElementAt(i).Housing.Longitude
                                );
                            GeoCoordinate searchDataGeoCoordinate = new GeoCoordinate(
                                    (double)data.Latitude,
                                    (double)data.Longitude
                                );
                            if (currentHousingGeoCoordinate.GetDistanceTo(searchDataGeoCoordinate) > data.Radius)
                            {
                                removeIndexes.Add(i);
                            }
                        }
                        foreach (var index in removeIndexes.OrderByDescending(item => item))
                        {
                            resultShareHousings.RemoveAt(index);
                        }
                    }

                    foreach (var item in resultShareHousings)
                    {
                        item.Housing.PhotoURLs = (from a in db.GetTable<Album>()
                                                  join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                  where (a.HousingID == item.Housing.ID)
                                                     && (a.CreatorID == item.Housing.Owner.UserID)
                                                  orderby p.PhotoID
                                                  select p.PhotoLink).ToList();
                    }
                }
                return resultShareHousings;
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
        public ShareHousingModel PostShareHousing(ShareHousingModel shareHousing)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var city = (from c in db.GetTable<City>()
                                where c.CityName == shareHousing.Housing.AddressCity
                                select c).SingleOrDefault();
                    if (city != null)
                    {
                        var oldAddress = (from a in db.GetTable<Address>()
                                          where (object.Equals(a.HouseNumber, shareHousing.Housing.AddressHouseNumber))
                                             && (object.Equals(a.Street, shareHousing.Housing.AddressStreet))
                                             && (object.Equals(a.Ward, shareHousing.Housing.AddressWard))
                                             && (a.District == shareHousing.Housing.AddressDistrict)
                                             && (a.CityID == city.CityID)
                                          select a).SingleOrDefault();

                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == shareHousing.Housing.ID
                                          select h).SingleOrDefault();
                        var oldShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where (sh.HousingID == shareHousing.Housing.ID)
                                                  && (sh.CreatorID == currentUser.UserID)
                                               select sh).SingleOrDefault();
                        if (oldAddress != null)
                        {
                            if (oldHousing != null)
                            {
                                // User created both Share and Housing. => User has right to update existent Housing and Share.
                                if (oldHousing.OwnerID == currentUser.UserID)
                                {
                                    var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                          where gl.LocationID == oldAddress.LocationID
                                                          select gl).SingleOrDefault();
                                    //if (oldGeoLocation != null)
                                    //{
                                    //    oldGeoLocation.Latitude = shareHousing.Housing.Latitude;
                                    //    oldGeoLocation.Longitude = shareHousing.Housing.Longitude;
                                    //    db.SubmitChanges();
                                    //}
                                    //else
                                    if (oldGeoLocation == null)
                                    {
                                        GeoLocation newGeoLocation = new GeoLocation
                                        {
                                            Latitude = shareHousing.Housing.Latitude,
                                            Longitude = shareHousing.Housing.Longitude
                                        };
                                        db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                        db.SubmitChanges();

                                        oldAddress.LocationID = newGeoLocation.LocationID;
                                        db.SubmitChanges();
                                    }

                                    // oldAddress exists. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = shareHousing.Housing.Title;
                                    oldHousing.Price = shareHousing.Housing.Price;
                                    oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == shareHousing.Housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                    oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                    oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                    oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                    oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                    oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                    oldHousing.HasAC = shareHousing.Housing.HasAC;
                                    oldHousing.HasParking = shareHousing.Housing.HasParking;
                                    oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                    oldHousing.Area = shareHousing.Housing.Area;

                                    oldHousing.AddressID = oldAddress.AddressID;

                                    oldHousing.Description = shareHousing.Housing.Description;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = shareHousing.Housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture
                                            var usCulture = new CultureInfo("en-US");
                                            if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = shareHousing.Housing.Title,
                                                        PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                            if (oldShareHousing != null)    // Update existent share Housing.
                                            {
                                                oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                oldShareHousing.Description = shareHousing.Description;
                                                oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                    where g.GenderType == shareHousing.RequiredGender
                                                                                    select g.GenderID).SingleOrDefault();
                                                oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                  where w.WorkType == shareHousing.RequiredWorkType
                                                                                  select w.WorkID).SingleOrDefault();
                                                oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldShareHousing.DateTimeExpired = null;

                                                shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                db.SubmitChanges();
                                                return shareHousing;
                                            }
                                        }
                                    }
                                }
                                // User create share from existent Housing. => No right to update existent Housing.
                                else
                                {
                                    // Update existent Share.
                                    if (oldShareHousing != null)
                                    {
                                        oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                        oldShareHousing.Description = shareHousing.Description;
                                        oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                        oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                            where g.GenderType == shareHousing.RequiredGender
                                                                            select g.GenderID).SingleOrDefault();
                                        oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                          where w.WorkType == shareHousing.RequiredWorkType
                                                                          select w.WorkID).SingleOrDefault();
                                        oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                        oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                        oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                        oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldShareHousing.DateTimeExpired = null;

                                        shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                        db.SubmitChanges();
                                    }
                                    // Create new Share from existent Housing.
                                    else
                                    {
                                        ShareHousing newShareHousing = new ShareHousing
                                        {
                                            HousingID = oldHousing.HousingID,
                                            CreatorID = currentUser.UserID,
                                            IsAvailable = true,
                                            PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                            Description = shareHousing.Description,
                                            NumOfView = 0,
                                            NumOfSaved = 0,
                                            RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                            RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                where g.GenderType == shareHousing.RequiredGender
                                                                select g.GenderID).SingleOrDefault(),
                                            RequiredWorkID = (from w in db.GetTable<Work>()
                                                              where w.WorkType == shareHousing.RequiredWorkType
                                                              select w.WorkID).SingleOrDefault(),
                                            AllowSmoking = shareHousing.AllowSmoking,
                                            AllowAlcohol = shareHousing.AllowAlcohol,
                                            HasPrivateKey = shareHousing.HasPrivateKey,
                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                            DateTimeExpired = null
                                        };
                                        db.ShareHousings.InsertOnSubmit(newShareHousing);
                                        db.SubmitChanges();

                                        shareHousing.ID = newShareHousing.ShareHousingID;
                                        shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                    }
                                    return shareHousing;
                                }
                            }
                            // oldHousing not exist. Create new Housing and Share.
                            else
                            {
                                var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                      where gl.LocationID == oldAddress.LocationID
                                                      select gl).SingleOrDefault();
                                //if (oldGeoLocation != null)
                                //{
                                //    oldGeoLocation.Latitude = shareHousing.Housing.Latitude;
                                //    oldGeoLocation.Longitude = shareHousing.Housing.Longitude;
                                //    db.SubmitChanges();
                                //}
                                //else
                                if (oldGeoLocation == null)
                                {
                                    GeoLocation newGeoLocation = new GeoLocation
                                    {
                                        Latitude = shareHousing.Housing.Latitude,
                                        Longitude = shareHousing.Housing.Longitude
                                    };
                                    db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                    db.SubmitChanges();

                                    oldAddress.LocationID = newGeoLocation.LocationID;
                                    db.SubmitChanges();
                                }

                                Housing newHousing = new Housing
                                {
                                    Title = shareHousing.Housing.Title,
                                    OwnerID = currentUser.UserID,
                                    Price = shareHousing.Housing.Price,
                                    IsAvailable = true,
                                    IsExist = false,    // User create both Share and Housing => Not real Housing Owner => Don't show on Housing Feed.
                                    HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                   where ht.HousingType == shareHousing.Housing.HouseType
                                                   select ht.HouseTypeID).SingleOrDefault(),
                                    DateTimeCreated = DateTimeOffset.UtcNow,
                                    DateTimeExpired = null,
                                    NumOfView = 0,
                                    NumOfSaved = 0,
                                    NumOfPeople = shareHousing.Housing.NumOfPeople,
                                    NumOfRoom = shareHousing.Housing.NumOfRoom,
                                    NumOfBed = shareHousing.Housing.NumOfBed,
                                    NumOfBath = shareHousing.Housing.NumOfBath,
                                    AllowPet = shareHousing.Housing.AllowPet,
                                    HasWifi = shareHousing.Housing.HasWifi,
                                    HasAC = shareHousing.Housing.HasAC,
                                    HasParking = shareHousing.Housing.HasParking,
                                    TimeRestriction = shareHousing.Housing.TimeRestriction,
                                    Area = shareHousing.Housing.Area,

                                    AddressID = oldAddress.AddressID,

                                    Description = shareHousing.Description,
                                    NumOfComment = 0
                                };
                                db.Housings.InsertOnSubmit(newHousing);
                                db.SubmitChanges();

                                Album newAlbum = new Album
                                {
                                    CreatorID = currentUser.UserID,
                                    HousingID = newHousing.HousingID,
                                    AlbumName = shareHousing.Housing.Title,
                                    DateTimeCreated = DateTimeOffset.UtcNow
                                };
                                db.Albums.InsertOnSubmit(newAlbum);
                                db.SubmitChanges();

                                // US culture.
                                var usCulture = new CultureInfo("en-US");
                                List<Photo> newPhotos = new List<Photo>();
                                for (int i = 0; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                        Description = shareHousing.Housing.Title,
                                        PhotoLink = shareHousing.Housing.PhotoURLs[i]
                                    };
                                    newPhotos.Add(newPhoto);
                                }
                                db.Photos.InsertAllOnSubmit(newPhotos);
                                db.SubmitChanges();

                                ShareHousing newShareHousing = new ShareHousing
                                {
                                    HousingID = newHousing.HousingID,
                                    CreatorID = currentUser.UserID,
                                    IsAvailable = true,
                                    PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                    Description = shareHousing.Description,
                                    NumOfView = 0,
                                    NumOfSaved = 0,
                                    RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                    RequiredGenderID = (from g in db.GetTable<Gender>()
                                                        where g.GenderType == shareHousing.RequiredGender
                                                        select g.GenderID).SingleOrDefault(),
                                    RequiredWorkID = (from w in db.GetTable<Work>()
                                                      where w.WorkType == shareHousing.RequiredWorkType
                                                      select w.WorkID).SingleOrDefault(),
                                    AllowSmoking = shareHousing.AllowSmoking,
                                    AllowAlcohol = shareHousing.AllowAlcohol,
                                    HasPrivateKey = shareHousing.HasPrivateKey,
                                    DateTimeCreated = DateTimeOffset.UtcNow,
                                    DateTimeExpired = null
                                };
                                db.ShareHousings.InsertOnSubmit(newShareHousing);
                                db.SubmitChanges();

                                //var recentlyInsertedAlbum = (from a in db.GetTable<Album>()
                                //                             where (a.CreatorID == currentUser.UserID)
                                //                                && (a.HousingID == newHousing.HousingID)
                                //                             select a).SingleOrDefault();
                                //newAlbum.ShareHousingID = newShareHousing.ShareHousingID;
                                //db.SubmitChanges();

                                shareHousing.Housing.ID = newHousing.HousingID;
                                shareHousing.Housing.DateTimeCreated = newHousing.DateTimeCreated;

                                shareHousing.ID = newShareHousing.ShareHousingID;
                                shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                return shareHousing;
                            }
                        }
                        // oldAddress not exist. Create new Address.
                        else
                        {
                            var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                  where (gl.Latitude == shareHousing.Housing.Latitude)
                                                     && (gl.Longitude == shareHousing.Housing.Longitude)
                                                  select gl).SingleOrDefault();
                            if (oldGeoLocation != null)
                            {
                                Address newAddress = new Address
                                {
                                    HouseNumber = shareHousing.Housing.AddressHouseNumber,
                                    Street = shareHousing.Housing.AddressStreet,
                                    Ward = shareHousing.Housing.AddressWard,
                                    District = shareHousing.Housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = oldGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // User created both Share and Housing. => User has right to update existent Housing And Share.
                                    if (oldHousing.OwnerID == currentUser.UserID)
                                    {
                                        // oldAddress exists. oldHousing exists. Update existent Housing.
                                        oldHousing.Title = shareHousing.Housing.Title;
                                        oldHousing.Price = shareHousing.Housing.Price;
                                        oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                        oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                                  where ht.HousingType == shareHousing.Housing.HouseType
                                                                  select ht.HouseTypeID).SingleOrDefault();
                                        oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousing.DateTimeExpired = null;
                                        oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                        oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                        oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                        oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                        oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                        oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                        oldHousing.HasAC = shareHousing.Housing.HasAC;
                                        oldHousing.HasParking = shareHousing.Housing.HasParking;
                                        oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                        oldHousing.Area = shareHousing.Housing.Area;

                                        oldHousing.AddressID = newAddress.AddressID;

                                        oldHousing.Description = shareHousing.Housing.Description;

                                        // Update existent Album.
                                        var oldAlbum = (from a in db.GetTable<Album>()
                                                        where (a.HousingID == oldHousing.HousingID)
                                                           && (a.CreatorID == currentUser.UserID)
                                                        select a).SingleOrDefault();
                                        if (oldAlbum != null)
                                        {
                                            oldAlbum.AlbumName = shareHousing.Housing.Title;
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
                                                if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    List<Photo> newPhotos = new List<Photo>();
                                                    for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                    {
                                                        String photoName = "IMG_" + currentUser.UID + "_"
                                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                        Photo newPhoto = new Photo
                                                        {
                                                            PhotoName = photoName,
                                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                                            AlbumID = oldAlbum.AlbumID,
                                                            Description = shareHousing.Housing.Title,
                                                            PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                        };
                                                        newPhotos.Add(newPhoto);
                                                    }
                                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                                {
                                                    int i = 0;
                                                    for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    for (int j = i; j < oldPhotos.Count; ++j)
                                                    {
                                                        db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                    }
                                                }
                                                db.SubmitChanges();

                                                shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                                if (oldShareHousing != null)    // Update existent share Housing.
                                                {
                                                    oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                    oldShareHousing.Description = shareHousing.Description;
                                                    oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                    oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                        where g.GenderType == shareHousing.RequiredGender
                                                                                        select g.GenderID).SingleOrDefault();
                                                    oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                      where w.WorkType == shareHousing.RequiredWorkType
                                                                                      select w.WorkID).SingleOrDefault();
                                                    oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                    oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                    oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                    oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldShareHousing.DateTimeExpired = null;

                                                    shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                    db.SubmitChanges();
                                                    return shareHousing;
                                                }
                                            }
                                        }
                                    }
                                    // User create Share from existent Housing. => No right to update existent Housing.
                                    else
                                    {
                                        // Update existent Share.
                                        if (oldShareHousing != null)
                                        {
                                            oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                            oldShareHousing.Description = shareHousing.Description;
                                            oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                            oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                where g.GenderType == shareHousing.RequiredGender
                                                                                select g.GenderID).SingleOrDefault();
                                            oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                              where w.WorkType == shareHousing.RequiredWorkType
                                                                              select w.WorkID).SingleOrDefault();
                                            oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                            oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                            oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                            oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousing.DateTimeExpired = null;

                                            shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                            db.SubmitChanges();
                                        }
                                        // Create new Share from existent Housing.
                                        else
                                        {
                                            ShareHousing newShareHousing = new ShareHousing
                                            {
                                                HousingID = oldHousing.HousingID,
                                                CreatorID = currentUser.UserID,
                                                IsAvailable = true,
                                                PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                                Description = shareHousing.Description,
                                                NumOfView = 0,
                                                NumOfSaved = 0,
                                                RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                                RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                    where g.GenderType == shareHousing.RequiredGender
                                                                    select g.GenderID).SingleOrDefault(),
                                                RequiredWorkID = (from w in db.GetTable<Work>()
                                                                  where w.WorkType == shareHousing.RequiredWorkType
                                                                  select w.WorkID).SingleOrDefault(),
                                                AllowSmoking = shareHousing.AllowSmoking,
                                                AllowAlcohol = shareHousing.AllowAlcohol,
                                                HasPrivateKey = shareHousing.HasPrivateKey,
                                                DateTimeCreated = DateTimeOffset.UtcNow,
                                                DateTimeExpired = null
                                            };
                                            db.ShareHousings.InsertOnSubmit(newShareHousing);
                                            db.SubmitChanges();

                                            shareHousing.ID = newShareHousing.ShareHousingID;
                                            shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                        }
                                        return shareHousing;
                                    }
                                }
                                // oldHousing not exist. New Address created. Create new Housing and Share.
                                else
                                {
                                    Housing newHousing = new Housing
                                    {
                                        Title = shareHousing.Housing.Title,
                                        OwnerID = currentUser.UserID,
                                        Price = shareHousing.Housing.Price,
                                        IsAvailable = true,
                                        IsExist = false,    // User create both Share and Housing => Not real Housing Owner => Don't show on Housing Feed.
                                        HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                       where ht.HousingType == shareHousing.Housing.HouseType
                                                       select ht.HouseTypeID).SingleOrDefault(),
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        NumOfPeople = shareHousing.Housing.NumOfPeople,
                                        NumOfRoom = shareHousing.Housing.NumOfRoom,
                                        NumOfBed = shareHousing.Housing.NumOfBed,
                                        NumOfBath = shareHousing.Housing.NumOfBath,
                                        AllowPet = shareHousing.Housing.AllowPet,
                                        HasWifi = shareHousing.Housing.HasWifi,
                                        HasAC = shareHousing.Housing.HasAC,
                                        HasParking = shareHousing.Housing.HasParking,
                                        TimeRestriction = shareHousing.Housing.TimeRestriction,
                                        Area = shareHousing.Housing.Area,

                                        AddressID = newAddress.AddressID,

                                        Description = shareHousing.Description,
                                        NumOfComment = 0
                                    };
                                    db.Housings.InsertOnSubmit(newHousing);
                                    db.SubmitChanges();

                                    Album newAlbum = new Album
                                    {
                                        CreatorID = currentUser.UserID,
                                        HousingID = newHousing.HousingID,
                                        AlbumName = shareHousing.Housing.Title,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    };
                                    db.Albums.InsertOnSubmit(newAlbum);
                                    db.SubmitChanges();

                                    // US culture
                                    var usCulture = new CultureInfo("en-US");
                                    List<Photo> newPhotos = new List<Photo>();
                                    for (int i = 0; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                            Description = shareHousing.Housing.Title,
                                            PhotoLink = shareHousing.Housing.PhotoURLs[i]
                                        };
                                        newPhotos.Add(newPhoto);
                                    }
                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                    db.SubmitChanges();

                                    ShareHousing newShareHousing = new ShareHousing
                                    {
                                        HousingID = newHousing.HousingID,
                                        CreatorID = currentUser.UserID,
                                        IsAvailable = true,
                                        PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                        Description = shareHousing.Description,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                        RequiredGenderID = (from g in db.GetTable<Gender>()
                                                            where g.GenderType == shareHousing.RequiredGender
                                                            select g.GenderID).SingleOrDefault(),
                                        RequiredWorkID = (from w in db.GetTable<Work>()
                                                          where w.WorkType == shareHousing.RequiredWorkType
                                                          select w.WorkID).SingleOrDefault(),
                                        AllowSmoking = shareHousing.AllowSmoking,
                                        AllowAlcohol = shareHousing.AllowAlcohol,
                                        HasPrivateKey = shareHousing.HasPrivateKey,
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null
                                    };
                                    db.ShareHousings.InsertOnSubmit(newShareHousing);
                                    db.SubmitChanges();

                                    //newAlbum.ShareHousingID = newShareHousing.ShareHousingID;
                                    //db.SubmitChanges();

                                    shareHousing.Housing.ID = newHousing.HousingID;
                                    shareHousing.Housing.DateTimeCreated = newHousing.DateTimeCreated;

                                    shareHousing.ID = newShareHousing.ShareHousingID;
                                    shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                    return shareHousing;
                                }
                            }
                            else    // oldGeoLocation not exist. Create newGeoLocation.
                            {
                                GeoLocation newGeoLocation = new GeoLocation
                                {
                                    Latitude = shareHousing.Housing.Latitude,
                                    Longitude = shareHousing.Housing.Longitude
                                };
                                db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                db.SubmitChanges();

                                Address newAddress = new Address
                                {
                                    HouseNumber = shareHousing.Housing.AddressHouseNumber,
                                    Street = shareHousing.Housing.AddressStreet,
                                    Ward = shareHousing.Housing.AddressWard,
                                    District = shareHousing.Housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = newGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // User created both Share and Housing. => User has right to update existent Housing And Share.
                                    if (oldHousing.OwnerID == currentUser.UserID)
                                    {
                                        // oldAddress exists. oldHousing exists. Update existent Housing.
                                        oldHousing.Title = shareHousing.Housing.Title;
                                        oldHousing.Price = shareHousing.Housing.Price;
                                        oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                        oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                                  where ht.HousingType == shareHousing.Housing.HouseType
                                                                  select ht.HouseTypeID).SingleOrDefault();
                                        oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousing.DateTimeExpired = null;
                                        oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                        oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                        oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                        oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                        oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                        oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                        oldHousing.HasAC = shareHousing.Housing.HasAC;
                                        oldHousing.HasParking = shareHousing.Housing.HasParking;
                                        oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                        oldHousing.Area = shareHousing.Housing.Area;

                                        oldHousing.AddressID = newAddress.AddressID;

                                        oldHousing.Description = shareHousing.Housing.Description;

                                        // Update existent Album.
                                        var oldAlbum = (from a in db.GetTable<Album>()
                                                        where (a.HousingID == oldHousing.HousingID)
                                                           && (a.CreatorID == currentUser.UserID)
                                                        select a).SingleOrDefault();
                                        if (oldAlbum != null)
                                        {
                                            oldAlbum.AlbumName = shareHousing.Housing.Title;
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
                                                if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    List<Photo> newPhotos = new List<Photo>();
                                                    for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                    {
                                                        String photoName = "IMG_" + currentUser.UID + "_"
                                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                        Photo newPhoto = new Photo
                                                        {
                                                            PhotoName = photoName,
                                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                                            AlbumID = oldAlbum.AlbumID,
                                                            Description = shareHousing.Housing.Title,
                                                            PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                        };
                                                        newPhotos.Add(newPhoto);
                                                    }
                                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                                {
                                                    int i = 0;
                                                    for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    for (int j = i; j < oldPhotos.Count; ++j)
                                                    {
                                                        db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                    }
                                                }
                                                db.SubmitChanges();

                                                shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                                if (oldShareHousing != null)    // Update existent share Housing.
                                                {
                                                    oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                    oldShareHousing.Description = shareHousing.Description;
                                                    oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                    oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                        where g.GenderType == shareHousing.RequiredGender
                                                                                        select g.GenderID).SingleOrDefault();
                                                    oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                      where w.WorkType == shareHousing.RequiredWorkType
                                                                                      select w.WorkID).SingleOrDefault();
                                                    oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                    oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                    oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                    oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldShareHousing.DateTimeExpired = null;

                                                    shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                    db.SubmitChanges();
                                                    return shareHousing;
                                                }
                                            }
                                        }
                                    }
                                    // User create Share from existent Housing. => No right to update existent Housing.
                                    else
                                    {
                                        // Update existent Share.
                                        if (oldShareHousing != null)
                                        {
                                            oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                            oldShareHousing.Description = shareHousing.Description;
                                            oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                            oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                where g.GenderType == shareHousing.RequiredGender
                                                                                select g.GenderID).SingleOrDefault();
                                            oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                              where w.WorkType == shareHousing.RequiredWorkType
                                                                              select w.WorkID).SingleOrDefault();
                                            oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                            oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                            oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                            oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousing.DateTimeExpired = null;

                                            shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                            db.SubmitChanges();
                                        }
                                        // Create new Share from existent Housing.
                                        else
                                        {
                                            ShareHousing newShareHousing = new ShareHousing
                                            {
                                                HousingID = oldHousing.HousingID,
                                                CreatorID = currentUser.UserID,
                                                IsAvailable = true,
                                                PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                                Description = shareHousing.Description,
                                                NumOfView = 0,
                                                NumOfSaved = 0,
                                                RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                                RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                    where g.GenderType == shareHousing.RequiredGender
                                                                    select g.GenderID).SingleOrDefault(),
                                                RequiredWorkID = (from w in db.GetTable<Work>()
                                                                  where w.WorkType == shareHousing.RequiredWorkType
                                                                  select w.WorkID).SingleOrDefault(),
                                                AllowSmoking = shareHousing.AllowSmoking,
                                                AllowAlcohol = shareHousing.AllowAlcohol,
                                                HasPrivateKey = shareHousing.HasPrivateKey,
                                                DateTimeCreated = DateTimeOffset.UtcNow,
                                                DateTimeExpired = null
                                            };
                                            db.ShareHousings.InsertOnSubmit(newShareHousing);
                                            db.SubmitChanges();

                                            shareHousing.ID = newShareHousing.ShareHousingID;
                                            shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                        }
                                        return shareHousing;
                                    }
                                }
                                // oldHousing not exist. New Address created. Create new Housing and Share.
                                else
                                {
                                    Housing newHousing = new Housing
                                    {
                                        Title = shareHousing.Housing.Title,
                                        OwnerID = currentUser.UserID,
                                        Price = shareHousing.Housing.Price,
                                        IsAvailable = true,
                                        IsExist = false,    // User create both Share and Housing => Not real Housing Owner => Don't show on Housing Feed.
                                        HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                       where ht.HousingType == shareHousing.Housing.HouseType
                                                       select ht.HouseTypeID).SingleOrDefault(),
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        NumOfPeople = shareHousing.Housing.NumOfPeople,
                                        NumOfRoom = shareHousing.Housing.NumOfRoom,
                                        NumOfBed = shareHousing.Housing.NumOfBed,
                                        NumOfBath = shareHousing.Housing.NumOfBath,
                                        AllowPet = shareHousing.Housing.AllowPet,
                                        HasWifi = shareHousing.Housing.HasWifi,
                                        HasAC = shareHousing.Housing.HasAC,
                                        HasParking = shareHousing.Housing.HasParking,
                                        TimeRestriction = shareHousing.Housing.TimeRestriction,
                                        Area = shareHousing.Housing.Area,

                                        AddressID = newAddress.AddressID,

                                        Description = shareHousing.Description,
                                        NumOfComment = 0
                                    };
                                    db.Housings.InsertOnSubmit(newHousing);
                                    db.SubmitChanges();

                                    Album newAlbum = new Album
                                    {
                                        CreatorID = currentUser.UserID,
                                        HousingID = newHousing.HousingID,
                                        AlbumName = shareHousing.Housing.Title,
                                        DateTimeCreated = DateTimeOffset.UtcNow
                                    };
                                    db.Albums.InsertOnSubmit(newAlbum);
                                    db.SubmitChanges();

                                    // US culture
                                    var usCulture = new CultureInfo("en-US");
                                    List<Photo> newPhotos = new List<Photo>();
                                    for (int i = 0; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                            Description = shareHousing.Housing.Title,
                                            PhotoLink = shareHousing.Housing.PhotoURLs[i]
                                        };
                                        newPhotos.Add(newPhoto);
                                    }
                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                    db.SubmitChanges();

                                    ShareHousing newShareHousing = new ShareHousing
                                    {
                                        HousingID = newHousing.HousingID,
                                        CreatorID = currentUser.UserID,
                                        IsAvailable = true,
                                        PricePerMonthOfOne = shareHousing.PricePerMonthOfOne,
                                        Description = shareHousing.Description,
                                        NumOfView = 0,
                                        NumOfSaved = 0,
                                        RequiredNumOfPeople = shareHousing.RequiredNumOfPeople,
                                        RequiredGenderID = (from g in db.GetTable<Gender>()
                                                            where g.GenderType == shareHousing.RequiredGender
                                                            select g.GenderID).SingleOrDefault(),
                                        RequiredWorkID = (from w in db.GetTable<Work>()
                                                          where w.WorkType == shareHousing.RequiredWorkType
                                                          select w.WorkID).SingleOrDefault(),
                                        AllowSmoking = shareHousing.AllowSmoking,
                                        AllowAlcohol = shareHousing.AllowAlcohol,
                                        HasPrivateKey = shareHousing.HasPrivateKey,
                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                        DateTimeExpired = null
                                    };
                                    db.ShareHousings.InsertOnSubmit(newShareHousing);
                                    db.SubmitChanges();

                                    //newAlbum.ShareHousingID = newShareHousing.ShareHousingID;
                                    //db.SubmitChanges();

                                    shareHousing.Housing.ID = newHousing.HousingID;
                                    shareHousing.Housing.DateTimeCreated = newHousing.DateTimeCreated;

                                    shareHousing.ID = newShareHousing.ShareHousingID;
                                    shareHousing.DateTimeCreated = newShareHousing.DateTimeCreated;
                                    return shareHousing;
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
        public bool UpdateShareHousing(ShareHousingModel shareHousing)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var city = (from c in db.GetTable<City>()
                                where c.CityName == shareHousing.Housing.AddressCity
                                select c).SingleOrDefault();
                    if (city != null)
                    {
                        var oldAddress = (from a in db.GetTable<Address>()
                                          where (object.Equals(a.HouseNumber, shareHousing.Housing.AddressHouseNumber))
                                             && (object.Equals(a.Street, shareHousing.Housing.AddressStreet))
                                             && (object.Equals(a.Ward, shareHousing.Housing.AddressWard))
                                             && (a.District == shareHousing.Housing.AddressDistrict)
                                             && (a.CityID == city.CityID)
                                          select a).SingleOrDefault();

                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == shareHousing.Housing.ID
                                          select h).SingleOrDefault();
                        var oldShareHousing = (from sh in db.GetTable<ShareHousing>()
                                               where (sh.HousingID == shareHousing.Housing.ID)
                                                  && (sh.CreatorID == currentUser.UserID)
                                               select sh).SingleOrDefault();
                        if (oldAddress != null)
                        {
                            if (oldHousing != null)
                            {
                                // User created both Share and Housing. => User has right to update existent Housing And Share.
                                if (oldHousing.OwnerID == currentUser.UserID)
                                {
                                    var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                          where gl.LocationID == oldAddress.LocationID
                                                          select gl).SingleOrDefault();
                                    //if (oldGeoLocation != null)
                                    //{
                                    //    oldGeoLocation.Latitude = shareHousing.Housing.Latitude;
                                    //    oldGeoLocation.Longitude = shareHousing.Housing.Longitude;
                                    //    db.SubmitChanges();
                                    //}
                                    //else
                                    if (oldGeoLocation == null)
                                    {
                                        GeoLocation newGeoLocation = new GeoLocation
                                        {
                                            Latitude = shareHousing.Housing.Latitude,
                                            Longitude = shareHousing.Housing.Longitude
                                        };
                                        db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                        db.SubmitChanges();

                                        oldAddress.LocationID = newGeoLocation.LocationID;
                                        db.SubmitChanges();
                                    }

                                    // oldAddress exists. oldHousing exists. Update existent Housing.
                                    oldHousing.Title = shareHousing.Housing.Title;
                                    oldHousing.Price = shareHousing.Housing.Price;
                                    oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                    oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                              where ht.HousingType == shareHousing.Housing.HouseType
                                                              select ht.HouseTypeID).SingleOrDefault();
                                    oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                    oldHousing.DateTimeExpired = null;
                                    oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                    oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                    oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                    oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                    oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                    oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                    oldHousing.HasAC = shareHousing.Housing.HasAC;
                                    oldHousing.HasParking = shareHousing.Housing.HasParking;
                                    oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                    oldHousing.Area = shareHousing.Housing.Area;

                                    oldHousing.AddressID = oldAddress.AddressID;

                                    oldHousing.Description = shareHousing.Housing.Description;

                                    // Update existent Album.
                                    var oldAlbum = (from a in db.GetTable<Album>()
                                                    where (a.HousingID == oldHousing.HousingID)
                                                       && (a.CreatorID == currentUser.UserID)
                                                    select a).SingleOrDefault();
                                    if (oldAlbum != null)
                                    {
                                        oldAlbum.AlbumName = shareHousing.Housing.Title;
                                        oldAlbum.DateTimeCreated = DateTimeOffset.UtcNow;

                                        // Update existent Photos.
                                        var oldPhotos = (from p in db.GetTable<Photo>()
                                                         where p.AlbumID == oldAlbum.AlbumID
                                                         orderby p.PhotoID
                                                         select p).ToList();
                                        if (oldPhotos != null && oldPhotos.Count > 0)
                                        {
                                            // US culture
                                            var usCulture = new CultureInfo("en-US");
                                            if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                            }
                                            else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                                List<Photo> newPhotos = new List<Photo>();
                                                for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                {
                                                    String photoName = "IMG_" + currentUser.UID + "_"
                                                        + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                    Photo newPhoto = new Photo
                                                    {
                                                        PhotoName = photoName,
                                                        DateTimeCreated = DateTimeOffset.UtcNow,
                                                        AlbumID = oldAlbum.AlbumID,
                                                        Description = shareHousing.Housing.Title,
                                                        PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                    };
                                                    newPhotos.Add(newPhoto);
                                                }
                                                db.Photos.InsertAllOnSubmit(newPhotos);
                                            }
                                            else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                            {
                                                int i = 0;
                                                for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                    oldPhotos[i].Description = shareHousing.Housing.Title;
                                                    oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                }
                                                for (int j = i; j < oldPhotos.Count; ++j)
                                                {
                                                    db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                }
                                            }
                                            db.SubmitChanges();

                                            shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                            if (oldShareHousing != null)    // Update existent share Housing.
                                            {
                                                oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                oldShareHousing.Description = shareHousing.Description;
                                                oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                    where g.GenderType == shareHousing.RequiredGender
                                                                                    select g.GenderID).SingleOrDefault();
                                                oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                  where w.WorkType == shareHousing.RequiredWorkType
                                                                                  select w.WorkID).SingleOrDefault();
                                                oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                oldShareHousing.DateTimeExpired = null;

                                                shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                db.SubmitChanges();
                                                return true;
                                            }
                                        }
                                    }
                                }
                                // User create Share from existent Housing. => No right to update existent Housing.
                                else
                                {
                                    // Update existent Share.
                                    if (oldShareHousing != null)
                                    {
                                        oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                        oldShareHousing.Description = shareHousing.Description;
                                        oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                        oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                            where g.GenderType == shareHousing.RequiredGender
                                                                            select g.GenderID).SingleOrDefault();
                                        oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                          where w.WorkType == shareHousing.RequiredWorkType
                                                                          select w.WorkID).SingleOrDefault();
                                        oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                        oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                        oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                        oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldShareHousing.DateTimeExpired = null;

                                        shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                        db.SubmitChanges();
                                        return true;
                                    }
                                }
                            }
                        }
                        // oldHousing not exist. oldAddress not exist. Create new Address.
                        else
                        {
                            var oldGeoLocation = (from gl in db.GetTable<GeoLocation>()
                                                  where (gl.Latitude == shareHousing.Housing.Latitude)
                                                     && (gl.Longitude == shareHousing.Housing.Longitude)
                                                  select gl).SingleOrDefault();
                            if (oldGeoLocation != null)
                            {
                                Address newAddress = new Address
                                {
                                    HouseNumber = shareHousing.Housing.AddressHouseNumber,
                                    Street = shareHousing.Housing.AddressStreet,
                                    Ward = shareHousing.Housing.AddressWard,
                                    District = shareHousing.Housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = oldGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // User created both Share and Housing. => User has right to update existent Housing And Share.
                                    if (oldHousing.OwnerID == currentUser.UserID)
                                    {
                                        // oldAddress exists. oldHousing exists. Update existent Housing.
                                        oldHousing.Title = shareHousing.Housing.Title;
                                        oldHousing.Price = shareHousing.Housing.Price;
                                        oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                        oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                                  where ht.HousingType == shareHousing.Housing.HouseType
                                                                  select ht.HouseTypeID).SingleOrDefault();
                                        oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousing.DateTimeExpired = null;
                                        oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                        oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                        oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                        oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                        oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                        oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                        oldHousing.HasAC = shareHousing.Housing.HasAC;
                                        oldHousing.HasParking = shareHousing.Housing.HasParking;
                                        oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                        oldHousing.Area = shareHousing.Housing.Area;

                                        oldHousing.AddressID = newAddress.AddressID;

                                        oldHousing.Description = shareHousing.Housing.Description;

                                        // Update existent Album.
                                        var oldAlbum = (from a in db.GetTable<Album>()
                                                        where (a.HousingID == oldHousing.HousingID)
                                                           && (a.CreatorID == currentUser.UserID)
                                                        select a).SingleOrDefault();
                                        if (oldAlbum != null)
                                        {
                                            oldAlbum.AlbumName = shareHousing.Housing.Title;
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
                                                if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    List<Photo> newPhotos = new List<Photo>();
                                                    for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                    {
                                                        String photoName = "IMG_" + currentUser.UID + "_"
                                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                        Photo newPhoto = new Photo
                                                        {
                                                            PhotoName = photoName,
                                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                                            AlbumID = oldAlbum.AlbumID,
                                                            Description = shareHousing.Housing.Title,
                                                            PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                        };
                                                        newPhotos.Add(newPhoto);
                                                    }
                                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                                {
                                                    int i = 0;
                                                    for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    for (int j = i; j < oldPhotos.Count; ++j)
                                                    {
                                                        db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                    }
                                                }
                                                db.SubmitChanges();

                                                shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                                if (oldShareHousing != null)    // Update existent share Housing.
                                                {
                                                    oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                    oldShareHousing.Description = shareHousing.Description;
                                                    oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                    oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                        where g.GenderType == shareHousing.RequiredGender
                                                                                        select g.GenderID).SingleOrDefault();
                                                    oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                      where w.WorkType == shareHousing.RequiredWorkType
                                                                                      select w.WorkID).SingleOrDefault();
                                                    oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                    oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                    oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                    oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldShareHousing.DateTimeExpired = null;

                                                    shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                    db.SubmitChanges();
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                    // User create share from existent Housing. => No right to update existent Housing.
                                    else
                                    {
                                        // Update existent Share.
                                        if (oldShareHousing != null)
                                        {
                                            oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                            oldShareHousing.Description = shareHousing.Description;
                                            oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                            oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                where g.GenderType == shareHousing.RequiredGender
                                                                                select g.GenderID).SingleOrDefault();
                                            oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                              where w.WorkType == shareHousing.RequiredWorkType
                                                                              select w.WorkID).SingleOrDefault();
                                            oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                            oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                            oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                            oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousing.DateTimeExpired = null;

                                            db.SubmitChanges();
                                            shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                            return true;
                                        }
                                    }
                                }
                            }
                            else    // oldGeoLocation not exist. Create newGeoLocation.
                            {
                                GeoLocation newGeoLocation = new GeoLocation
                                {
                                    Latitude = shareHousing.Housing.Latitude,
                                    Longitude = shareHousing.Housing.Longitude
                                };
                                db.GeoLocations.InsertOnSubmit(newGeoLocation);
                                db.SubmitChanges();

                                Address newAddress = new Address
                                {
                                    HouseNumber = shareHousing.Housing.AddressHouseNumber,
                                    Street = shareHousing.Housing.AddressStreet,
                                    Ward = shareHousing.Housing.AddressWard,
                                    District = shareHousing.Housing.AddressDistrict,
                                    CityID = city.CityID,
                                    LocationID = newGeoLocation.LocationID
                                };
                                db.Addresses.InsertOnSubmit(newAddress);
                                db.SubmitChanges();

                                if (oldHousing != null)
                                {
                                    // User created both Share and Housing. => User has right to update existent Housing And Share.
                                    if (oldHousing.OwnerID == currentUser.UserID)
                                    {
                                        // oldAddress exists. oldHousing exists. Update existent Housing.
                                        oldHousing.Title = shareHousing.Housing.Title;
                                        oldHousing.Price = shareHousing.Housing.Price;
                                        oldHousing.IsAvailable = shareHousing.Housing.IsAvailable;
                                        oldHousing.HouseTypeID = (from ht in db.GetTable<HouseType>()
                                                                  where ht.HousingType == shareHousing.Housing.HouseType
                                                                  select ht.HouseTypeID).SingleOrDefault();
                                        oldHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                        oldHousing.DateTimeExpired = null;
                                        oldHousing.NumOfPeople = shareHousing.Housing.NumOfPeople;
                                        oldHousing.NumOfRoom = shareHousing.Housing.NumOfRoom;
                                        oldHousing.NumOfBed = shareHousing.Housing.NumOfBed;
                                        oldHousing.NumOfBath = shareHousing.Housing.NumOfBath;
                                        oldHousing.AllowPet = shareHousing.Housing.AllowPet;
                                        oldHousing.HasWifi = shareHousing.Housing.HasWifi;
                                        oldHousing.HasAC = shareHousing.Housing.HasAC;
                                        oldHousing.HasParking = shareHousing.Housing.HasParking;
                                        oldHousing.TimeRestriction = shareHousing.Housing.TimeRestriction;
                                        oldHousing.Area = shareHousing.Housing.Area;

                                        oldHousing.AddressID = newAddress.AddressID;

                                        oldHousing.Description = shareHousing.Housing.Description;

                                        // Update existent Album.
                                        var oldAlbum = (from a in db.GetTable<Album>()
                                                        where (a.HousingID == oldHousing.HousingID)
                                                           && (a.CreatorID == currentUser.UserID)
                                                        select a).SingleOrDefault();
                                        if (oldAlbum != null)
                                        {
                                            oldAlbum.AlbumName = shareHousing.Housing.Title;
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
                                                if (shareHousing.Housing.PhotoURLs.Count == oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count > oldPhotos.Count)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    List<Photo> newPhotos = new List<Photo>();
                                                    for (int j = i; j < shareHousing.Housing.PhotoURLs.Count; ++j)
                                                    {
                                                        String photoName = "IMG_" + currentUser.UID + "_"
                                                            + DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss", usCulture) + "_" + j;

                                                        Photo newPhoto = new Photo
                                                        {
                                                            PhotoName = photoName,
                                                            DateTimeCreated = DateTimeOffset.UtcNow,
                                                            AlbumID = oldAlbum.AlbumID,
                                                            Description = shareHousing.Housing.Title,
                                                            PhotoLink = shareHousing.Housing.PhotoURLs[j]
                                                        };
                                                        newPhotos.Add(newPhoto);
                                                    }
                                                    db.Photos.InsertAllOnSubmit(newPhotos);
                                                }
                                                else if (shareHousing.Housing.PhotoURLs.Count < oldPhotos.Count)
                                                {
                                                    int i = 0;
                                                    for (; i < shareHousing.Housing.PhotoURLs.Count; ++i)
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
                                                        oldPhotos[i].Description = shareHousing.Housing.Title;
                                                        oldPhotos[i].PhotoLink = shareHousing.Housing.PhotoURLs[i];
                                                    }
                                                    for (int j = i; j < oldPhotos.Count; ++j)
                                                    {
                                                        db.Photos.DeleteOnSubmit(oldPhotos[j]);
                                                    }
                                                }
                                                db.SubmitChanges();

                                                shareHousing.Housing.DateTimeCreated = oldHousing.DateTimeCreated;

                                                if (oldShareHousing != null)    // Update existent share Housing.
                                                {
                                                    oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                                    oldShareHousing.Description = shareHousing.Description;
                                                    oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                                    oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                        where g.GenderType == shareHousing.RequiredGender
                                                                                        select g.GenderID).SingleOrDefault();
                                                    oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                                      where w.WorkType == shareHousing.RequiredWorkType
                                                                                      select w.WorkID).SingleOrDefault();
                                                    oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                                    oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                                    oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                                    oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                                    oldShareHousing.DateTimeExpired = null;

                                                    shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
                                                    db.SubmitChanges();
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                    // User create share from existent Housing. => No right to update existent Housing.
                                    else
                                    {
                                        // Update existent Share.
                                        if (oldShareHousing != null)
                                        {
                                            oldShareHousing.PricePerMonthOfOne = shareHousing.PricePerMonthOfOne;
                                            oldShareHousing.Description = shareHousing.Description;
                                            oldShareHousing.RequiredNumOfPeople = shareHousing.RequiredNumOfPeople;
                                            oldShareHousing.RequiredGenderID = (from g in db.GetTable<Gender>()
                                                                                where g.GenderType == shareHousing.RequiredGender
                                                                                select g.GenderID).SingleOrDefault();
                                            oldShareHousing.RequiredWorkID = (from w in db.GetTable<Work>()
                                                                              where w.WorkType == shareHousing.RequiredWorkType
                                                                              select w.WorkID).SingleOrDefault();
                                            oldShareHousing.AllowSmoking = shareHousing.AllowSmoking;
                                            oldShareHousing.AllowAlcohol = shareHousing.AllowAlcohol;
                                            oldShareHousing.HasPrivateKey = shareHousing.HasPrivateKey;
                                            oldShareHousing.DateTimeCreated = DateTimeOffset.UtcNow;
                                            oldShareHousing.DateTimeExpired = null;

                                            db.SubmitChanges();
                                            shareHousing.DateTimeCreated = oldShareHousing.DateTimeCreated;
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
        public bool DeleteShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldShareHousing = (from sh in db.GetTable<ShareHousing>()
                                           where (sh.HousingID == shareHousingID)
                                              && (sh.CreatorID == currentUser.UserID)
                                           select sh).SingleOrDefault();
                    if (oldShareHousing != null)
                    {
                        var oldHousing = (from h in db.GetTable<Housing>()
                                          where h.HousingID == oldShareHousing.HousingID
                                          select h).SingleOrDefault();
                        if (oldHousing != null)
                        {
                            // User created both Share and Housing. Delete both.
                            if (oldHousing.OwnerID == currentUser.UserID)
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

                                        // Delete all user's saved housings.
                                        var oldSavedHousing = (from s in db.GetTable<SavedHousing>()
                                                               where s.HousingID == oldHousing.HousingID
                                                               select s).ToList();
                                        if (oldSavedHousing != null && oldSavedHousing.Count > 0)
                                        {
                                            db.SavedHousings.DeleteAllOnSubmit(oldSavedHousing);
                                            db.SubmitChanges();
                                        }

                                        // Delete all user's Share posts of current Housing.
                                        var oldShareHousings = (from s in db.GetTable<ShareHousing>()
                                                                where s.HousingID == oldHousing.HousingID
                                                                select s).ToList();
                                        if (oldShareHousings != null && oldShareHousings.Count > 0)
                                        {
                                            foreach (var item in oldShareHousings)
                                            {
                                                // Delete all user's saved share housings.
                                                var oldSavedShareHousing = (from ss in db.GetTable<SavedShareHousing>()
                                                                            where ss.ShareHousingID == item.ShareHousingID
                                                                            select ss).ToList();
                                                if (oldSavedShareHousing != null && oldSavedShareHousing.Count > 0)
                                                {
                                                    db.SavedShareHousings.DeleteAllOnSubmit(oldSavedShareHousing);
                                                    db.SubmitChanges();
                                                }

                                                // Delete all user's appointment of current Share Housing.
                                                var oldShareHousingAppointment = (from a in db.GetTable<ShareHousingAppointment>()
                                                                                  where a.ShareHousingID == item.ShareHousingID
                                                                                  select a).ToList();
                                                if (oldShareHousingAppointment != null && oldShareHousingAppointment.Count > 0)
                                                {
                                                    db.ShareHousingAppointments.DeleteAllOnSubmit(oldShareHousingAppointment);
                                                    db.SubmitChanges();
                                                }
                                            }
                                            db.ShareHousings.DeleteAllOnSubmit(oldShareHousings);
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
                                        var oldAppointment = (from a in db.GetTable<HousingAppointment>()
                                                              where a.HousingID == oldHousing.HousingID
                                                              select a).ToList();
                                        if (oldAppointment != null && oldAppointment.Count > 0)
                                        {
                                            db.HousingAppointments.DeleteAllOnSubmit(oldAppointment);
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
                            // User only created Share. Delete only Share.
                            else
                            {
                                // Delete all user's saved share housings.
                                var oldSavedShareHousing = (from ss in db.GetTable<SavedShareHousing>()
                                                            where ss.ShareHousingID == oldShareHousing.ShareHousingID
                                                            select ss).ToList();
                                if (oldSavedShareHousing != null && oldSavedShareHousing.Count > 0)
                                {
                                    db.SavedShareHousings.DeleteAllOnSubmit(oldSavedShareHousing);
                                    db.SubmitChanges();
                                }

                                // Delete all user's appointment of current Share Housing.
                                var oldShareHousingAppointment = (from a in db.GetTable<ShareHousingAppointment>()
                                                                  where a.ShareHousingID == oldShareHousing.ShareHousingID
                                                                  select a).ToList();
                                if (oldShareHousingAppointment != null && oldShareHousingAppointment.Count > 0)
                                {
                                    db.ShareHousingAppointments.DeleteAllOnSubmit(oldShareHousingAppointment);
                                    db.SubmitChanges();
                                }

                                db.ShareHousings.DeleteOnSubmit(oldShareHousing);
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

        [Route("")]
        [HttpGet]
        [Authorize]
        public bool CheckIfExistSharePostsOfCurrentHousing(int housingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldShareHousings = (from sh in db.GetTable<ShareHousing>()
                                            where (sh.HousingID == housingID)
                                            select sh).FirstOrDefault();
                    if (oldShareHousings != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Route("")]
        [HttpGet]
        [Authorize]
        public List<ShareHousingModel> GetMoreOlderShareOfHousing(int housingID, int offset)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var oldShareHousings = (from sh in db.GetTable<ShareHousing>()
                                            where (sh.HousingID == housingID)
                                            orderby sh.DateTimeCreated descending
                                            select sh).Skip(5 * offset).Take(5).ToList();
                    if (oldShareHousings != null && oldShareHousings.Count > 0)
                    {
                        List<ShareHousingModel> shareHousings = new List<ShareHousingModel>();
                        foreach (var item in oldShareHousings)
                        {
                            var currentHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == item.HousingID
                                                  select h).SingleOrDefault();
                            var currentHousingOwner = (from u in db.GetTable<ShareSpaceUser>()
                                                       where u.UserID == currentHousing.OwnerID
                                                       select u).SingleOrDefault();
                            var currentHousingAddress = (from a in db.GetTable<Address>()
                                                         join gl in db.GetTable<GeoLocation>() on a.LocationID equals gl.LocationID
                                                         join c in db.GetTable<City>() on a.CityID equals c.CityID
                                                         where a.AddressID == currentHousing.AddressID
                                                         select new { a, gl, c }).SingleOrDefault();
                            shareHousings.Add(new ShareHousingModel {
                                ID = item.ShareHousingID,
                                Housing = new HousingModel
                                {
                                    ID = currentHousing.HousingID,
                                    PhotoURLs = (from a in db.GetTable<Album>()
                                                 join p in db.GetTable<Photo>() on a.AlbumID equals p.AlbumID
                                                 where (a.HousingID == currentHousing.HousingID)
                                                    && (a.CreatorID == currentHousing.OwnerID)
                                                 orderby p.PhotoID
                                                 select p.PhotoLink).ToList(),
                                    Title = currentHousing.Title,
                                    Owner = new UserModel
                                    {
                                        UserID = currentHousingOwner.UserID,
                                        UID = currentHousingOwner.UID,
                                        FirstName = currentHousingOwner.FirstName,
                                        LastName = currentHousingOwner.LastName,
                                        Email = (from user in db.GetTable<ShareSpaceUser>()
                                                 join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                                 join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                                 where user.UserID == currentHousing.OwnerID
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
                                    Latitude = currentHousingAddress.gl.Latitude,
                                    Longitude = currentHousingAddress.gl.Longitude,
                                    AddressHouseNumber = currentHousingAddress.a.HouseNumber,
                                    AddressStreet = currentHousingAddress.a.Street,
                                    AddressWard = currentHousingAddress.a.Ward,
                                    AddressDistrict = currentHousingAddress.a.District,
                                    AddressCity = currentHousingAddress.c.CityName,
                                    Description = currentHousing.Description,
                                    NumOfComment = currentHousing.NumOfComment,
                                },
                                Creator = new UserModel
                                {
                                    UserID = currentHousingOwner.UserID,
                                    UID = currentHousingOwner.UID,
                                    FirstName = currentHousingOwner.FirstName,
                                    LastName = currentHousingOwner.LastName,
                                    Email = (from user in db.GetTable<ShareSpaceUser>()
                                             join e in db.GetTable<Email>() on user.EmailID equals e.EmailID
                                             join ed in db.GetTable<EmailDomain>() on e.DomainID equals ed.DomainID
                                             where user.UserID == item.CreatorID
                                             select e.LocalPart + "@" + ed.DomainName).SingleOrDefault(),
                                    DOB = currentHousingOwner.DOB,
                                    PhoneNumber = currentHousingOwner.PhoneNumber,
                                    Gender = (from u in db.GetTable<ShareSpaceUser>()
                                              join g in db.GetTable<Gender>() on u.GenderID equals g.GenderID
                                              where u.UserID == item.CreatorID
                                              select g.GenderType).SingleOrDefault(),
                                    NumOfNote = currentHousingOwner.NumOfNote,
                                    DeviceTokens = currentHousingOwner.DeviceTokens
                                },
                                IsAvailable = item.IsAvailable,
                                PricePerMonthOfOne = item.PricePerMonthOfOne,
                                Description = item.Description,
                                NumOfView = item.NumOfView,
                                NumOfSaved = item.NumOfSaved,
                                RequiredNumOfPeople = item.RequiredNumOfPeople,
                                RequiredGender = (from g in db.GetTable<Gender>()
                                                  where g.GenderID == item.RequiredGenderID
                                                  select g.GenderType).SingleOrDefault(),
                                RequiredWorkType = (from w in db.GetTable<Work>()
                                                    where w.WorkID == item.RequiredWorkID
                                                    select w.WorkType).SingleOrDefault(),
                                AllowSmoking = item.AllowSmoking,
                                AllowAlcohol = item.AllowAlcohol,
                                HasPrivateKey = item.HasPrivateKey,
                                DateTimeCreated = DateTimeOffset.UtcNow
                            });
                        }
                        return shareHousings;
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

        [Route("report")]
        [HttpPost]
        [Authorize]
        public bool ReportShareHousing(int shareHousingID)
        {
            try
            {
                UserModel currentUser = base.GetCurrentUserInfo();

                if (currentUser != null)
                {
                    DBShareSpaceDataContext db = new DBShareSpaceDataContext();

                    var currentShareHousing = (from h in db.GetTable<ShareHousing>()
                                               where h.ShareHousingID == shareHousingID
                                               select h).SingleOrDefault();
                    if (currentShareHousing != null)
                    {
                        if (currentUser.UserID != currentShareHousing.CreatorID)
                        {
                            var currentHousing = (from h in db.GetTable<Housing>()
                                                  where h.HousingID == currentShareHousing.HousingID
                                                  select h).SingleOrDefault();
                            if (currentHousing.OwnerID == currentShareHousing.CreatorID)
                            {
                                currentHousing.DateTimeExpired = DateTimeOffset.UtcNow.AddDays(3);
                            }
                            currentShareHousing.DateTimeExpired = DateTimeOffset.UtcNow.AddDays(3);    // This Share Housing will be expired in next 3 days.
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
    }
}
