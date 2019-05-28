using API.Bungie;
using Core.Models.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Web.BungieCache
{
	internal class ClanUpdater
	{
		internal void UpdateAllClans()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Clan timer go work");
			Console.ResetColor();
			var queue = new Queue<long>();
			var bungieApi = new BungieApi();

			using (FailsafeContext failsafe = new FailsafeContext())
			{
				//Grab all Destiny Clan ID and add to Queue
				foreach (var item in failsafe.Destiny2Clans.Select(C => new { C.Id }))
				{
					queue.Enqueue(item.Id);
				}

				while (queue.Count > 0)
				{
					var ClanId = queue.Dequeue();
					var ClanInfoDb = failsafe.Destiny2Clans.First(C => C.Id == ClanId);
					try
					{
						var ClanInfoBungie = bungieApi.GetGroupResult(ClanId);

						if (ClanInfoBungie == null)
							failsafe.Destiny2Clans.Remove(ClanInfoDb);
						else
						{
							ClanInfoDb.Name = ClanInfoBungie.Response.Detail.Name;
							ClanInfoDb.Motto = ClanInfoBungie.Response.Detail.Motto;
							ClanInfoDb.About = ClanInfoBungie.Response.Detail.About;
							ClanInfoDb.MemberCount = ClanInfoBungie.Response.Detail.MemberCount;
						}
						failsafe.Destiny2Clans.Update(ClanInfoDb);
						failsafe.SaveChanges();
					}
					catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				}
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Clan timer done work");
			Console.ResetColor();
		}

		internal void ClanMemberCheck()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Clan Member timer go work");
			Console.ResetColor();
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
						//Get list of all clan members form Bungie.
						var ClanMembersBungie = bungieApi.GetMembersOfGroupResponse(ClanId).Response.Results.ToList();
						//Get list of all clan members from local db
						var Members = failsafe.Destiny2Clan_Members.Where(M => M.Destiny2ClanId == ClanId);
						foreach (var item in ClanMembersBungie)
						{
							//Find DestinyMembershipId form local db in Bungie response
							var BungieData = Members.SingleOrDefault(B => B.DestinyMembershipId == item.DestinyUserInfo.MembershipId);
							//If not found Member add him
							if (BungieData == null)
							{
								var Member = new Destiny2Clan_Member
								{
									Name = item.DestinyUserInfo.DisplayName,
									DestinyMembershipType = item.DestinyUserInfo.MembershipType,
									DestinyMembershipId = item.DestinyUserInfo.MembershipId,
									ClanJoinDate = item.JoinDate,
									Destiny2ClanId = item.GroupId
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
					catch (Exception ex) { Console.WriteLine(ex.ToString()); }
				}
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - Clan Member timer done work");
			Console.ResetColor();
		}
	}
}
