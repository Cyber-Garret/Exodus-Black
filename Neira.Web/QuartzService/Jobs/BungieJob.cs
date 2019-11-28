using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Neira.API.Bungie;
using Neira.Web.Database;

using Quartz;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.QuartzService
{
	[DisallowConcurrentExecution]
	public class BungieJob : IJob
	{
		private readonly ILogger<BungieJob> _logger;
		public BungieJob(ILogger<BungieJob> logger)
		{
			_logger = logger;
		}

		public Task Execute(IJobExecutionContext context)
		{
			UpdateClans();
			ClanMemberCheck();
			UpdateMembersLastPlayedTime();
			return Task.CompletedTask;
		}

		private void UpdateClans()
		{
			_logger.LogInformation("Update Destiny clans start working at {RequestTime}", DateTime.Now);

			var bungieApi = new BungieApi();

			using var Db = new NeiraLinkContext();
			var queue = new Queue<Clan>(Db.Clans);
			while (queue.Count > 0)
			{
				var clan = queue.Dequeue();
				try
				{
					var ClanResponse = bungieApi.GetGroupResult(clan.Id);
					if (ClanResponse.ErrorCode == 1)
					{
						//var localData = db.Clans.First(c => c.Id == clan.Id);

						clan.Name = ClanResponse.Response.Detail.Name;
						clan.Motto = ClanResponse.Response.Detail.Motto;
						clan.About = ClanResponse.Response.Detail.About;
						clan.MemberCount = ClanResponse.Response.Detail.MemberCount;

						Db.Clans.Update(clan);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error in UpdateClansAsync method");
				}
			}
			Db.SaveChanges();

			_logger.LogInformation("Update Destiny clans complete working at {RequestTime}", DateTime.Now);
		}

		private void ClanMemberCheck()
		{
			_logger.LogInformation("Clan member check start working at {RequestTime}", DateTime.Now);

			var bungieApi = new BungieApi();

			using var Db = new NeiraLinkContext();
			var queue = new Queue<Clan>(Db.Clans.Include(m => m.Members));

			while (queue.Count > 0)
			{
				var clan = queue.Dequeue();
				try
				{
					//Get list of guardians in current guild
					var bungieResponse = bungieApi.GetMembersOfGroupResponse(clan.Id);

					//ErrorCode = 1 mean Success
					if (bungieResponse.ErrorCode != 1)
						continue;
					foreach (var result in bungieResponse.Response.Results)
					{
						var guardian = clan.Members.SingleOrDefault(g => g.DestinyMembershipId == result.DestinyUserInfo.MembershipId);

						if (guardian == null)
						{
							var clan_Member = new Clan_Member
							{
								Name = result.DestinyUserInfo.DisplayName,
								DestinyMembershipType = result.DestinyUserInfo.MembershipType,
								DestinyMembershipId = result.DestinyUserInfo.MembershipId,
								ClanJoinDate = result.JoinDate,
								ClanId = result.GroupId
							};
							if (result.BungieNetUserInfo != null)
							{
								clan_Member.BungieMembershipType = result.BungieNetUserInfo.MembershipType;
								clan_Member.BungieMembershipId = result.BungieNetUserInfo.MembershipId;
								clan_Member.IconPath = result.BungieNetUserInfo.IconPath;
							}
							Db.Add(clan_Member);
						}
						else
						{
							//If member have public profile
							if (result.BungieNetUserInfo != null)
							{
								guardian.BungieMembershipType = result.BungieNetUserInfo.MembershipType;
								guardian.BungieMembershipId = result.BungieNetUserInfo.MembershipId;
								guardian.IconPath = result.BungieNetUserInfo.IconPath;
							}
							//If member profile hidden
							else if (result.BungieNetUserInfo == null)
							{
								guardian.BungieMembershipType = null;
								guardian.BungieMembershipId = null;
								guardian.IconPath = null;
							}
							//Anyway just update profile Name
							guardian.Name = result.DestinyUserInfo.DisplayName;
							guardian.DestinyMembershipType = result.DestinyUserInfo.MembershipType;
							guardian.DestinyMembershipId = result.DestinyUserInfo.MembershipId;


							Db.Update(guardian);
						}
					}
					//Check and delete member from local DB who not in current guild
					foreach (var guardian in clan.Members)
					{
						var IsInClan = bungieResponse.Response.Results.Any(M => M.DestinyUserInfo.MembershipId == guardian.DestinyMembershipId && M.GroupId == guardian.ClanId);
						if (!IsInClan)
						{
							Db.Remove(guardian);
						}
					}
					Db.SaveChanges();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error in ClanMemberCheckAsync method.");
				}
			}

			_logger.LogInformation("Clan member check complete working at {RequestTime}", DateTime.Now);
		}

		private void UpdateMembersLastPlayedTime()
		{

			_logger.LogInformation("Update Guardian last online time start working at {RequestTime}", DateTime.Now);

			using var Db = new NeiraLinkContext();
			var queue = new Queue<Clan_Member>(Db.Clan_Members.AsNoTracking().OrderBy(g => g.DateLastPlayed));
			var bungieApi = new BungieApi();

			while (queue.Count > 0)
			{
				var guardian = queue.Dequeue();

				try
				{
					var profile = bungieApi.GetProfileResult(guardian.DestinyMembershipId, (int)guardian.DestinyMembershipType, DestinyComponentType.Profiles);

					if (profile.ErrorCode == 1 && profile != null)
					{
						var member = Db.Clan_Members.Single(m => m.DestinyMembershipId == guardian.DestinyMembershipId);

						member.DateLastPlayed = profile.Response.Profile.Data.DateLastPlayed;

						Db.Update(member);
						Db.SaveChanges();
					}
					else
						_logger.LogInformation($"Bungie error on {guardian.Name} Response:\n ErrorCode: {profile.ErrorCode}\n{profile.ErrorStatus} - {profile.Message}");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error in update last online time on Guardian {guardian.Name}.");
				}
			}
			_logger.LogInformation("Update Guardian last online time complete working at {RequestTime}", DateTime.Now);
		}
	}
}
