using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Discord;

using Core;
using Core.Models.Destiny2;
using API.Bungie;
namespace Bot.Services.Bungie
{
	internal class ClanUpdater
	{
		internal async void UpdateAllClans()
		{
			await Logger.Log(new LogMessage(LogSeverity.Info, "UpdateAllClans", "Start the work"));

			var queue = new Queue<long>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Destiny Clan ID and add to Queue
				foreach (var item in failsafe.Clans.Select(C => new { C.Id }))
				{
					queue.Enqueue(item.Id);
				}

				while (queue.Count > 0)
				{
					var ClanId = queue.Dequeue();
					var ClanInfoDb = failsafe.Clans.First(C => C.Id == ClanId);
					try
					{
						var ClanInfoBungie = bungieApi.GetGroupResult(ClanId);

						if (ClanInfoBungie == null)
							failsafe.Clans.Remove(ClanInfoDb);
						else
						{
							ClanInfoDb.Name = ClanInfoBungie.Response.Detail.Name;
							ClanInfoDb.Motto = ClanInfoBungie.Response.Detail.Motto;
							ClanInfoDb.About = ClanInfoBungie.Response.Detail.About;
							ClanInfoDb.MemberCount = ClanInfoBungie.Response.Detail.MemberCount;
						}
						failsafe.Clans.Update(ClanInfoDb);
						failsafe.SaveChanges();
					}
					catch (Exception ex) { await Logger.Log(new LogMessage(LogSeverity.Error, "UpdateAllClans", ex.Message, ex)); }
				}
			}
			await Logger.Log(new LogMessage(LogSeverity.Info, "UpdateAllClans", "Done the work"));
		}

		internal async void ClanMemberCheck()
		{
			await Logger.Log(new LogMessage(LogSeverity.Info, "ClanMemberCheck", "Start the work"));
			var ClanQueue = new Queue<long>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Destiny Clan ID and add to Queue
				foreach (var item in failsafe.Clans.Select(C => new { C.Id }))
				{
					ClanQueue.Enqueue(item.Id);
				}

				while (ClanQueue.Count > 0)
				{
					var ClanId = ClanQueue.Dequeue();
					try
					{
						//Get list of all clan members form Bungie.
						var ClanMembersBungie = bungieApi.GetMembersOfGroupResponse(ClanId).Response.Results.ToList();
						//Get list of all clan members from local db
						var Members = failsafe.Clan_Members.Where(M => M.ClanId == ClanId);
						foreach (var item in ClanMembersBungie)
						{
							//Find DestinyMembershipId form local db in Bungie response
							var BungieData = Members.SingleOrDefault(B => B.DestinyMembershipId == item.DestinyUserInfo.MembershipId);
							//If not found Member add him
							if (BungieData == null)
							{
								var Member = new Clan_Member
								{
									Name = item.DestinyUserInfo.DisplayName,
									DestinyMembershipType = item.DestinyUserInfo.MembershipType,
									DestinyMembershipId = item.DestinyUserInfo.MembershipId,
									ClanJoinDate = item.JoinDate,
									ClanId = item.GroupId
								};
								if (item.BungieNetUserInfo != null)
								{
									Member.BungieMembershipType = item.BungieNetUserInfo.MembershipType;
									Member.BungieMembershipId = item.BungieNetUserInfo.MembershipId;
									Member.IconPath = item.BungieNetUserInfo.IconPath;
								}
								failsafe.Add(Member);
							}
							//Anyway update member if earlier member profile closed or opened
							else if (BungieData != null)
							{
								//If member have public profile
								if (item.BungieNetUserInfo != null)
								{
									BungieData.BungieMembershipType = item.BungieNetUserInfo.MembershipType;
									BungieData.BungieMembershipId = item.BungieNetUserInfo.MembershipId;
									BungieData.IconPath = item.BungieNetUserInfo.IconPath;
								}
								//If member profile hidden
								else if (item.BungieNetUserInfo == null)
								{

									BungieData.BungieMembershipType = null;
									BungieData.BungieMembershipId = null;
									BungieData.IconPath = null;
								}
								//Anyway just update profile Name
								BungieData.Name = item.DestinyUserInfo.DisplayName;

								failsafe.Update(BungieData);
							}

							//Check and delete member from local DB who not in current guild
							foreach (var Member in Members)
							{
								var Deleted = ClanMembersBungie.Any(M => M.DestinyUserInfo.MembershipId == Member.DestinyMembershipId);
								if (!Deleted)
									failsafe.Remove(Member);
							}
							//Save all changes we made for profile
							failsafe.SaveChanges();
						}
					}
					catch (Exception ex) { await Logger.Log(new LogMessage(LogSeverity.Error, "ClanMemberCheck", ex.Message, ex)); }
				}
			}
			await Logger.Log(new LogMessage(LogSeverity.Info, "ClanMemberCheck", "Done the work"));
		}
	}
}
