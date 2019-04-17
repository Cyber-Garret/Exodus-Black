using System;
using System.Collections.Generic;
using System.Text;

namespace API.Bungie.Models
{
    public class GetProfileResult : RootObject
    {
        public Response1 Response { get; set; }
    }

    public class Response1
    {
        public Profile profile { get; set; }
        public ProfileCurrencies profileCurrencies { get; set; }
        public Itemcomponents itemComponents { get; set; }
    }

    public class ProfileCurrencies
    {
        public int privacy { get; set; }
    }


    public class Profile
    {
        public Data data { get; set; }
        public int privacy { get; set; }
    }

    public class Data
    {
        public Userinfo userInfo { get; set; }
        public DateTime dateLastPlayed { get; set; }
        public int versionsOwned { get; set; }
        public string[] characterIds { get; set; }
    }

    public class Userinfo
    {
        public int membershipType { get; set; }
        public string membershipId { get; set; }
        public string displayName { get; set; }
    }

    public class Itemcomponents
    {

    }

}
