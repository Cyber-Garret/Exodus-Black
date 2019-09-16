using Neira.API.Bungie;
using Neira.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neira.BungieWorker.Bungie
{
	internal class MemberUpdater
	{
		public void UpdateMembersLastPlayedTime()
		{
			const string MethodName = "[Update Members Last Played]";

			Logger.Log.Information($"{MethodName} Start the work");
			var bungieApi = new BungieApi();

			using (var Db = new NeiraContext())
			{

				Parallel.ForEach(Db.Clan_Members.OrderBy(M => M.DateLastPlayed), new ParallelOptions { MaxDegreeOfParallelism = 3 }, ProfileId =>
				   {
					   try
					   {
						   var profile = bungieApi.GetProfileResult(ProfileId.DestinyMembershipId, BungieMembershipType.TigerBlizzard, DestinyComponentType.Profiles);

						   var member = Db.Clan_Members.Single(m => m.DestinyMembershipId == ProfileId.DestinyMembershipId);

						   member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						   Db.Update(member);
					   }
					   catch (Exception ex)
					   {
						   Logger.Log.Error($"Error in {MethodName} on Guardian {ProfileId.Name}. Message: {ex.Message}");
					   }
				   });
				Db.SaveChanges();
			}
			Logger.Log.Information($"{MethodName} Done the work");
		}
	}
}
