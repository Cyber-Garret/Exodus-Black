using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using API.Bungie.Models;
using API.Bungie.Models.Account;
using API.Bungie.Account;
using System.Linq;

using API.Bungie.GroupV2;
using API.Bungie.Profile;
using API.Bungie.Models.GroupV2.GetGroup;

namespace API.Bungie
{
    public class Destiny2ClanMemberStat
    {
        private int _clanId { get; set; }
        private Dictionary<string, string> UserId { get; set; }
        private List<GuidMember> guidMembers { get; set; }
        public Destiny2ClanMemberStat(int ClanId)
        {
            _clanId = ClanId;
            UserId = new Dictionary<string, string>();
            guidMembers = new List<GuidMember>();
        }

        private async Task GetGuildMemberId()
        {
            GetAllMembers members = new GetAllMembers();
            var z = (await members.GetGuildMembers(_clanId)).Response;
            foreach (var item in z.Results)
            {
                if(item.BungieNetUserInfo == null)
                {
                    UserId.Add(item.DestinyUserInfo.MembershipId, string.Empty);
                }
                else
                {
                    UserId.Add(item.DestinyUserInfo.MembershipId, item.BungieNetUserInfo.MembershipId);
                }
            }

        }

        public async Task<List<GuidMember>> GetGuildCharacterIdsAsync()
        {
            try
            {
                await GetGuildMemberId();

                GetProfile profile = new GetProfile();

                foreach (var item in UserId)
                {
                    var User = (await profile.GetProfileWithComponentsAsync(BungieMembershipType.TigerBlizzard, item.Key, DestinyComponentType.Profiles)).Response;
                    guidMembers.Add(new GuidMember { ProfileId = item.Value, MemberName = User.profile.data.userInfo.displayName, LastOnlineDate = User.profile.data.dateLastPlayed });
                }
                return guidMembers.OrderBy(g => g.LastOnlineDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return guidMembers = null;
            }
        }

        public async Task<GetGroupResult> GuildInfoAsync(int GuildId)
        {
            GetAllMembers members = new GetAllMembers();
            var info = await members.GetGroup(GuildId);
            return info;
        }
    }
}
