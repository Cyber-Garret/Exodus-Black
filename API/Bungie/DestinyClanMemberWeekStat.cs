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
        private List<string> CharacterIds { get; set; }
        public DestinyClanMemberWeekStat(int ClanId)
        {
            _clanId = ClanId;
            UserId = new List<string>();
            CharacterIds = new List<string>();
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
                GetProfileResult User = profile.GetProfileWithComponentsAsync(BungieMembershipType.TigerBlizzard, item, DestinyComponentType.Profiles).Result;
                string[] characters = User.Response.profile.data.characterIds;
                foreach (string character in characters)
                {
                    GetAccount account = new GetAccount();
                    AccountResult wtf = await account.GetAccountAsync(User.Response.profile.data.userInfo.membershipId, character, 250, 0);
                    foreach (var activity in wtf.Response.Activities)
                    {
                        activity.Period.DateTime
                    }
                }
            }
            await Task.CompletedTask;
        }
    }
}
