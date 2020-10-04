using BungieAPI.User;

using System.Collections.Generic;

namespace BungieAPI.Responses
{
	public class DestinyLinkedProfilesResponse
	{
		public IEnumerable<DestinyProfileUserInfoCard> Profiles { get; set; }
		public UserInfoCard BNetMembership { get; set; }
		// profilesWithErrors
	}
}