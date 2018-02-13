using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareSpaceServer.Models
{
    public class ShareHousingModel
    {
        public int ID { get; set; }
        public HousingModel Housing { get; set; }
        public UserModel Creator { get; set; }
        public bool IsAvailable { get; set; }
        public int PricePerMonthOfOne { get; set; }
        public String Description { get; set; }
        public int NumOfView { get; set; }
        public int NumOfSaved { get; set; }
        public int RequiredNumOfPeople { get; set; }
        public String RequiredGender { get; set; }
        public String RequiredWorkType { get; set; }
        public bool AllowSmoking { get; set; }
        public bool AllowAlcohol { get; set; }
        public bool HasPrivateKey { get; set; }
        public DateTimeOffset DateTimeCreated { get; set; }
    }
}