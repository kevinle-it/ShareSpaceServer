using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class HousingModel
    {
        public int ID { get; set; }
        public List<String> PhotoURLs { get; set; }
        public String Title { get; set; }
        public UserModel Owner { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public String HouseType { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
        public int NumOfView { get; set; }
        public int NumOfSaved { get; set; }
        public int NumOfPeople { get; set; }
        public int NumOfRoom { get; set; }
        public int NumOfBed { get; set; }
        public int NumOfBath { get; set; }
        public bool AllowPet { get; set; }
        public bool HasWifi { get; set; }
        public bool HasAC { get; set; }
        public bool HasParking { get; set; }
        public TimeSpan? TimeRestriction { get; set; }
        public decimal Area { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public String AddressHouseNumber { get; set; }
        public String AddressStreet { get; set; }
        public String AddressWard { get; set; }
        public String AddressDistrict { get; set; }
        public String AddressCity { get; set; }
        public String Description { get; set; }
        //public NoteModel CurrentUserNote { get; set; }
        //public List<String> CurrentUserPhotoURLs { get; set; }
        //public DateTimeOffset CurrentUserAppointment { get; set; }
        //public String LatestCommentContent { get; set; }
        public int NumOfComment { get; set; }
        //public int DomainID { get; set; }
        //public String LocalPart { get; set; }
        //public int EmailID { get; set; }
    }
}