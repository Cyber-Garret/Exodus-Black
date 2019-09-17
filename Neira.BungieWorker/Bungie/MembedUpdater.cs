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
		private bool UpdatememberBusy { get; set; }
		private static readonly MemberUpdater instance = new MemberUpdater();

		private MemberUpdater()
		{
			UpdatememberBusy = false;
		}

		public static MemberUpdater GetInstance()
		{
			return instance;
		}

		public void UpdateMembersLastPlayedTime()
		{
			if (UpdatememberBusy) return;

			const string MethodName = "[Update Members Last Played]";

			Logger.Log.Information($"{MethodName} Start the work");
			UpdatememberBusy = true;
			var bungieApi = new BungieApi();

			using (var Db = new NeiraContext())
			{

				Parallel.ForEach(Db.Clan_Members.OrderBy(M => M.DateLastPlayed), new ParallelOptions { MaxDegreeOfParallelism = 2 }, ProfileId =>
				   {
					   try
					   {
						   using (var context = new NeiraContext())
						   {
							   BungieMembershipType type = (BungieMembershipType)Enum.ToObject(typeof(BungieMembershipType), ProfileId.DestinyMembershipType);

							   var profile = bungieApi.GetProfileResult(ProfileId.DestinyMembershipId, type, DestinyComponentType.Profiles);
							   if (profile != null)
							   {
								   var member = context.Clan_Members.SingleOrDefault(m => m.DestinyMembershipId == ProfileId.DestinyMembershipId);
								   if (member != null)
								   {
									   member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

									   context.Update(member);
								   }
							   }
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
			UpdatememberBusy = false;
		}
	}
}
