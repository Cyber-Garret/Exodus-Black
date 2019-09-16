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
						   using (var context = new NeiraContext())
						   {
							   BungieMembershipType type = (BungieMembershipType)Enum.ToObject(typeof(BungieMembershipType), ProfileId.DestinyMembershipType);

							   var profile = bungieApi.GetProfileResult(ProfileId.DestinyMembershipId, type, DestinyComponentType.Profiles);

							   var member = context.Clan_Members.Single(m => m.DestinyMembershipId == ProfileId.DestinyMembershipId);

							   member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

							   context.Update(member);
						   }
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
