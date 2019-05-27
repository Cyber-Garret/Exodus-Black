using API.Bungie;
using API.Bungie.Models;
using Core.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.BungieCache
{
	internal class MemberUpdater
	{
		public async Task UpdateClanMembersAsync()
		{
			var ClanQueue = new Queue<long>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Destiny Clan ID and add to Queue
				foreach (var item in failsafe.Destiny2Clans.Select(C => new { C.Id }))
				{
					ClanQueue.Enqueue(item.Id);
				}
				while (ClanQueue.Count > 0)
				{
					var ClanId = ClanQueue.Dequeue();

					try
					{
						var ClanMembersBungie = bungieApi.GetMembersOfGroupResponse(ClanId);
						var members = failsafe.Destiny2Clan_Members.ToList();

						foreach (var item in members)
						{
							var profile = bungieApi.GetProfileResult(item.DestinyMembershipId, BungieMembershipType.TigerBlizzard, DestinyComponentType.Profiles);

							var member = failsafe.Destiny2Clan_Members.Single(m => m.DestinyMembershipId == item.DestinyMembershipId);

							member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

							failsafe.Update(member);
						}
						await failsafe.SaveChangesAsync();
					}
					catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				}
			}

		}
	}
}
