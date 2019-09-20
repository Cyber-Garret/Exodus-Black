using Newtonsoft.Json;
using System;

namespace Neira.API.Bungie.Models.Results.GroupV2.GetMembersOfGroup
{
	public class GetMembersOfGroup : RootResult
	{
		[JsonProperty("Response")]
		public Response Response { get; set; }
	}

	public class Response
	{
		[JsonProperty("results")]
		public Result[] Results { get; set; }

		[JsonProperty("totalResults")]
		public long TotalResults { get; set; }

		[JsonProperty("hasMore")]
		public bool HasMore { get; set; }

		[JsonProperty("query")]
		public Query Query { get; set; }

		[JsonProperty("useTotalResults")]
		public bool UseTotalResults { get; set; }
	}

	public class Query
	{
		[JsonProperty("itemsPerPage")]
		public long ItemsPerPage { get; set; }

		[JsonProperty("currentPage")]
		public long CurrentPage { get; set; }
	}

	public class Result
	{
		[JsonProperty("memberType")]
		public long MemberType { get; set; }

		[JsonProperty("isOnline")]
		public bool IsOnline { get; set; }

		[JsonProperty("lastOnlineStatusChange")]
		public long LastOnlineStatusChange { get; set; }

		[JsonProperty("groupId")]
		public long GroupId { get; set; }

		[JsonProperty("destinyUserInfo")]
		public UserInfo DestinyUserInfo { get; set; }

		[JsonProperty("joinDate")]
		public DateTimeOffset JoinDate { get; set; }

		[JsonProperty("bungieNetUserInfo", NullValueHandling = NullValueHandling.Ignore)]
		public UserInfo BungieNetUserInfo { get; set; }
	}

	public class UserInfo
	{
		[JsonProperty("supplementalDisplayName", NullValueHandling = NullValueHandling.Ignore)]
		public string SupplementalDisplayName { get; set; }

		[JsonProperty("iconPath")]
		public string IconPath { get; set; }

		[JsonProperty("membershipType")]
		public long MembershipType { get; set; }

		[JsonProperty("membershipId")]
		public string MembershipId { get; set; }

		[JsonProperty("displayName")]
		public string DisplayName { get; set; }
	}
}
