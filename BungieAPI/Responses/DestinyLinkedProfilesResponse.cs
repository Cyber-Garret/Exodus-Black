using System.Collections.Generic;
using BungieAPI.User;

namespace BungieAPI.Responses
{
    public class DestinyLinkedProfilesResponse
    {
        public IEnumerable<DestinyProfileUserInfoCard> Profiles { get; set; }
        public UserInfoCard BNetMembership { get; set; }
        // profilesWithErrors
    }
}