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
		private static readonly ClanUpdater instance = new ClanUpdater();
		public bool UpdateClanBusy { get; private set; }
		public bool MemberCheckBusy { get; private set; }
		public bool UpdateMemberBusy { get; private set; }
		private ClanUpdater()
		{
			UpdateClanBusy = false;
			MemberCheckBusy = false;
			UpdateMemberBusy = false;
		}
		public static ClanUpdater GetInstance()
		{
			return instance;
		}
		internal void UpdateClans()
		{
			Logger.Log.Information("Update Destiny clans start working");
			UpdateClanBusy = true;


			var bungieApi = new BungieApi();

			using (var db = new NeiraContext())
			{
				var queue = new Queue<Clan>(db.Clans.AsNoTracking());
				while (queue.Count > 0)
				{
					var clan = queue.Dequeue();
					try
					{
						var ClanResponse = bungieApi.GetGroupResult(clan.Id);
						if (ClanResponse.ErrorCode == 1)
						{
							var localData = db.Clans.Single(c => c.Id == clan.Id);

							localData.Name = ClanResponse.Response.Detail.Name;
							localData.Motto = ClanResponse.Response.Detail.Motto;
							localData.About = ClanResponse.Response.Detail.About;
							localData.MemberCount = ClanResponse.Response.Detail.MemberCount;

							db.Clans.Update(clan);
							db.SaveChanges();
						}
					}
					catch (Exception ex)
					{
						Logger.Log.Error(ex, $"Error in UpdateClansAsync method");
					}
				}
			}
			Logger.Log.Information($"Update Destiny clans complete working");
			UpdateClanBusy = false;
		}

		internal async Task ClanMemberCheckAsync()
		{
			if (MemberCheckBusy) return;
			Logger.Log.Information($"Clan member check start working");
			MemberCheckBusy = true;

			var bungieApi = new BungieApi();

			using (var Db = new NeiraContext())
			{
				foreach (var clan in Db.Clans.Include(m => m.Members))
				{
					try
					{
						//Get member list from bungie
						var MemberResponse = bungieApi.GetMembersOfGroupResponse(clan.Id).Response.Results.ToList();

						foreach (var member in MemberResponse)
						{
							var BungieData = clan.Members.SingleOrDefault(u => u.DestinyMembershipId == member.DestinyUserInfo.MembershipId);
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
								Db.Add(Member);
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

								Db.Update(BungieData);
							}
							//Check and delete member from local DB who not in current guild
							foreach (var guardian in clan.Members)
							{
								var Deleted = MemberResponse.Any(M => M.DestinyUserInfo.MembershipId == guardian.DestinyMembershipId && M.GroupId == guardian.ClanId);
								if (!Deleted)
								{
									Db.Remove(guardian);
								}
							}

						}
						await Db.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						Logger.Log.Error(ex, "Error in ClanMemberCheckAsync method.");
					}
				}
			}

			Logger.Log.Information("Clan member check complete working");
			MemberCheckBusy = false;
		}

		public Task UpdateMembersLastPlayedTime()
		{
			if (UpdateMemberBusy) return Task.CompletedTask;

			Logger.Log.Information($"Update Guardian last online time start working");
			UpdateMemberBusy = true;

			using (var Db = new NeiraContext())
			{

				Parallel.ForEach(Db.Clan_Members.OrderBy(M => M.DateLastPlayed), new ParallelOptions { MaxDegreeOfParallelism = 3 }, async ProfileId =>
				{
					try
					{
						using (var context = new NeiraContext())
						{
							BungieMembershipType type = (BungieMembershipType)Enum.ToObject(typeof(BungieMembershipType), ProfileId.DestinyMembershipType);

							var bungieApi = new BungieApi();
							var profile = bungieApi.GetProfileResult(ProfileId.DestinyMembershipId, type, DestinyComponentType.Profiles);

							if (profile.ErrorCode == 1)
							{
								var member = context.Clan_Members.SingleOrDefault(m => m.DestinyMembershipId == ProfileId.DestinyMembershipId);
								if (member != null)
								{
									member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

									context.Update(member);
									await context.SaveChangesAsync();
								}
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Log.Fatal(ex, $"Error in update last online time on Guardian {ProfileId.Name}.");
					}
				});
			}
			Logger.Log.Information($"Update Guardian last online time complete working");
			UpdateMemberBusy = false;
			return Task.CompletedTask;
		}
	}
}
