using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Text;

using Discord;

using Core;
using API.Bungie;
using API.Bungie.Models;

namespace Bot.Services.Bungie
{
	internal class MemberUpdater
	{
		public async void UpdateMembersLastPlayedTime()
		{
			await Logger.Log(new LogMessage(LogSeverity.Info, "UpdateMembersLastPlayedTime", "Start the work"));

			var MemberQueue = new Queue<string>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Member profile ID and add to Queue
				foreach (var item in failsafe.Clan_Members.OrderBy(M => M.DateLastPlayed))
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

						var member = failsafe.Clan_Members.Single(m => m.DestinyMembershipId == ProfileId);

						member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						failsafe.Update(member);

						failsafe.SaveChanges();
					}
					catch (Exception ex) { await Logger.Log(new LogMessage(LogSeverity.Error, "UpdateMembersLastPlayedTime", ex.Message, ex)); }
				}
			}
			await Logger.Log(new LogMessage(LogSeverity.Info, "UpdateMembersLastPlayedTime", "Done the work"));
		}
	}
}
