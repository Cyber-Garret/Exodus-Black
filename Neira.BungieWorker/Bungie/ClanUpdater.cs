using Microsoft.EntityFrameworkCore;
using Neira.API.Bungie;
using Neira.Db;
using Neira.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neira.BungieWorker.Bungie
{
	internal class ClanUpdater
	{
		private bool UpdateClanBusy { get; set; }
		private bool MemberCheckBusy { get; set; }
		private static readonly ClanUpdater instance = new ClanUpdater();
		private ClanUpdater()
		{
			UpdateClanBusy = false;
			MemberCheckBusy = false;
		}
		public static ClanUpdater GetInstance()
		{
			return instance;
		}
		internal void UpdateAllClans()
		{
			if (UpdateClanBusy) return;
			const string MethodName = "[Update All Clans]";

			Logger.Log.Information($"{MethodName} Start the wotk");
			UpdateClanBusy = true;
			var bungieApi = new BungieApi();

			using (var db = new NeiraContext())
			{
				Parallel.ForEach(db.Clans.Select(c => new { c.Id }), new ParallelOptions { MaxDegreeOfParallelism = 2 }, clan =>
				{
					try
					{
						using (var context = new NeiraContext())
						{
							var ClanInfoDb = context.Clans.First(C => C.Id == clan.Id);

							var ClanInfoBungie = bungieApi.GetGroupResult(clan.Id);
							if (ClanInfoBungie == null)
								context.Clans.Remove(ClanInfoDb);
							else
							{
								ClanInfoDb.Name = ClanInfoBungie.Response.Detail.Name;
								ClanInfoDb.Motto = ClanInfoBungie.Response.Detail.Motto;
								ClanInfoDb.About = ClanInfoBungie.Response.Detail.About;
								ClanInfoDb.MemberCount = ClanInfoBungie.Response.Detail.MemberCount;
							}
							context.Clans.Update(ClanInfoDb);
							context.SaveChanges();
						}
					}
					catch (Exception ex)
					{
						Logger.Log.Error(ex, $"{MethodName} {ex.Message}");
					}
				});
			}
			Logger.Log.Information($"{MethodName} Done the work");
			UpdateClanBusy = false;
		}

		internal void ClanMemberCheck()
		{
			if (MemberCheckBusy) return;

			const string MethodName = "[Clan Member Check]";
			Logger.Log.Information($"{MethodName} Start the work");
			MemberCheckBusy = true;
			var bungieApi = new BungieApi();

			using (var Db = new NeiraContext())
			{
				Parallel.ForEach(Db.Clans.Select(c => new { c.Id }), new ParallelOptions { MaxDegreeOfParallelism = 3 }, async clan =>
				{
					try
					{
						using (var context = new NeiraContext())
						{
							//Get list of all clan members form Bungie.
							var ClanMembersBungie = bungieApi.GetMembersOfGroupResponse(clan.Id).Response.Results.ToList();
							//Get list of all clan members from local db
							var Members = await context.Clan_Members.Where(M => M.ClanId == clan.Id).ToListAsync();

							Parallel.ForEach(ClanMembersBungie, new ParallelOptions { MaxDegreeOfParallelism = 3 }, member =>
							{
								//Find DestinyMembershipId form local db in Bungie response
								var BungieData = Members.SingleOrDefault(B => B.DestinyMembershipId == member.DestinyUserInfo.MembershipId);
								//If not found Member add him
								if (BungieData == null)
								{
									var Member = new Clan_Member
									{
										Name = member.DestinyUserInfo.DisplayName,
										DestinyMembershipType = member.DestinyUserInfo.MembershipType,
										DestinyMembershipId = member.DestinyUserInfo.MembershipId,
										ClanJoinDate = member.JoinDate,
										ClanId = member.GroupId
									};
									if (member.BungieNetUserInfo != null)
									{
										Member.BungieMembershipType = member.BungieNetUserInfo.MembershipType;
										Member.BungieMembershipId = member.BungieNetUserInfo.MembershipId;
										Member.IconPath = member.BungieNetUserInfo.IconPath;
									}
									context.Add(Member);
								}
								//Anyway update member if earlier member profile closed or opened
								else if (BungieData != null)
								{
									//If member have public profile
									if (member.BungieNetUserInfo != null)
									{
										BungieData.BungieMembershipType = member.BungieNetUserInfo.MembershipType;
										BungieData.BungieMembershipId = member.BungieNetUserInfo.MembershipId;
										BungieData.IconPath = member.BungieNetUserInfo.IconPath;
									}
									//If member profile hidden
									else if (member.BungieNetUserInfo == null)
									{

										BungieData.BungieMembershipType = null;
										BungieData.BungieMembershipId = null;
										BungieData.IconPath = null;
									}
									//Anyway just update profile Name
									BungieData.Name = member.DestinyUserInfo.DisplayName;

									context.Update(BungieData);
								}
								//Check and delete member from local DB who not in current guild
								foreach (var Member in Members)
								{
									var Deleted = ClanMembersBungie.Any(M => M.DestinyUserInfo.MembershipId == Member.DestinyMembershipId);
									if (!Deleted)
										context.Remove(Member);
								}
							});
							context.SaveChanges();
						}
					}
					catch (Exception ex)
					{
						Logger.Log.Error(ex, $"{MethodName} {ex.Message}");
					}
				});
			}
			Logger.Log.Information($"{MethodName} Done the work");
			MemberCheckBusy = false;
		}
	}
}
