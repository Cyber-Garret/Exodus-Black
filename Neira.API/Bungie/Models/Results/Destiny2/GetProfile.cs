using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neira.API.Bungie.Models.Results.Destiny2.GetProfile
{
	/// <summary>
	/// Returns Destiny Profile information for the supplied membership.
	/// </summary>
	public class GetProfile : RootResult
	{
		//Verb: GET
		//Path: /Destiny2/{membershipType}/Profile/{destinyMembershipId}
		//Returns Destiny Profile information for the supplied membership.

		[JsonProperty("Response")]
		public Response Response { get; set; }
	}

	public class Response
	{
		[JsonProperty("profile")]
		public Profile Profile { get; set; }
	}

	public partial class Profile
	{
		[JsonProperty("data")]
		public Data Data { get; set; }

		[JsonProperty("privacy")]
		public long Privacy { get; set; }
	}

	public class Data
	{
		[JsonProperty("userInfo")]
		public UserInfo UserInfo { get; set; }

		[JsonProperty("dateLastPlayed")]
		public DateTimeOffset DateLastPlayed { get; set; }

		[JsonProperty("versionsOwned")]
		public long VersionsOwned { get; set; }

		[JsonProperty("characterIds")]
		public string[] CharacterIds { get; set; }
	}

	public class UserInfo
	{
		[JsonProperty("membershipType")]
		public long MembershipType { get; set; }

		[JsonProperty("membershipId")]
		public string MembershipId { get; set; }

		[JsonProperty("displayName")]
		public string DisplayName { get; set; }
	}
}
