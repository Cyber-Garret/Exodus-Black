using API.Bungie.GroupV2;
using API.Bungie.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using API.Bungie.Models;
using API.Bungie.Models.Account;
using API.Bungie.Account;

namespace API.Bungie
{
    public class DestinyClanMemberWeekStat
    {
        private int _clanId { get; set; }
        private List<string> UserId { get; set; }
        private Dictionary<string, DateTime> GuildMemberDict { get; set; }
        public DestinyClanMemberWeekStat(int ClanId)
        {
            _clanId = ClanId;
            UserId = new List<string>();
            GuildMemberDict = new Dictionary<string, DateTime>();
        }

        public async Task GetGuildMemberId()
        {
            GetAllMembers members = new GetAllMembers();
            var z = (await members.GetGuildMembers(_clanId)).Response;
            foreach (var item in z.Results)
            {
                UserId.Add(item.DestinyUserInfo.MembershipId);
            }

        }

        public async Task GetGuildCharacterIds()
        {
            GetProfile profile = new GetProfile();

            foreach (var item in UserId)
            {
                var User = (await profile.GetProfileWithComponentsAsync(BungieMembershipType.TigerBlizzard, item, DestinyComponentType.Profiles)).Response;
                GuildMemberDict.Add(User.profile.data.userInfo.displayName, User.profile.data.dateLastPlayed);
            }
            await Task.CompletedTask;
        }
    }
}
