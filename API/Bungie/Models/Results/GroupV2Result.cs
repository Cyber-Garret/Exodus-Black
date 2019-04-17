using System;
using System.Collections.Generic;
using System.Text;

namespace API.Bungie.Models
{
    public class GroupV2Result : RootObject
    {
        public Response Response { get; set; }
    }

    public class Response
    {
        public List<Result> results { get; set; }
        public int totalResults { get; set; }
        public bool hasMore { get; set; }
        public Query query { get; set; }
        public bool useTotalResults { get; set; }
    }

    public class Query
    {
        public int itemsPerPage { get; set; }
        public int currentPage { get; set; }
    }

    public class Result
    {
        public int memberType { get; set; }
        public bool isOnline { get; set; }
        public string lastOnlineStatusChange { get; set; }
        public string groupId { get; set; }
        public DestinyUserInfo destinyUserInfo { get; set; }
        public DateTime joinDate { get; set; }
        public BungieNetUserInfo bungieNetUserInfo { get; set; }
    }

    public class BungieNetUserInfo
    {
        public string supplementalDisplayName { get; set; }
        public string iconPath { get; set; }
        public int membershipType { get; set; }
        public string membershipId { get; set; }
        public string displayName { get; set; }
    }

    public class DestinyUserInfo
    {
        public string iconPath { get; set; }
        public int membershipType { get; set; }
        public string membershipId { get; set; }
        public string displayName { get; set; }
    }
}
