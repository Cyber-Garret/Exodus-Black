using API.Bungie;
using API.Bungie.Models;
using Core.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Web.BungieCache
{
	internal class MemberUpdater
	{
		public void UpdateMembersLastPlayedTime()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Update Member timer go work");
			Console.ResetColor();

			var MemberQueue = new Queue<string>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Member profile ID and add to Queue
				foreach (var item in failsafe.Destiny2Clan_Members.OrderBy(M => M.DateLastPlayed))
				{
					MemberQueue.Enqueue(item.DestinyMembershipId);
				}

				while (MemberQueue.Count > 0)
				{
					var ProfileId = MemberQueue.Dequeue();

					try
					{
						Thread.Sleep(1000);
						var profile = bungieApi.GetProfileResult(ProfileId, BungieMembershipType.TigerBlizzard, DestinyComponentType.Profiles);

						var member = failsafe.Destiny2Clan_Members.Single(m => m.DestinyMembershipId == ProfileId);

						member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						failsafe.Update(member);

						failsafe.SaveChanges();
					}
					catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				}
			}
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Update Member timer done work");
			Console.ResetColor();

		}
	}
}
