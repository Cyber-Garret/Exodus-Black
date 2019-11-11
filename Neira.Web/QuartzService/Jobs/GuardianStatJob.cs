using Microsoft.Extensions.Logging;
using Neira.API.Bungie;
using Neira.Web.Models.NeiraLink;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Web.QuartzService
{
	[DisallowConcurrentExecution]
	public class GuardianStatJob : IJob
	{
		private readonly ILogger<GuardianStatJob> logger;

		public GuardianStatJob(ILogger<GuardianStatJob> logger)
		{
			this.logger = logger;
		}
		public Task Execute(IJobExecutionContext context)
		{
			GrabGuardianPlayedTime();
			return Task.CompletedTask;
		}

		private void GrabGuardianPlayedTime()
		{
			using var Db = new NeiraContext();
			var guardians = Db.Clan_Members.ToList();

			var BungieApi = new BungieApi();

			foreach (var guardian in guardians)
			{
				var stat = new Clan_Member_Stat
				{
					MemberId = guardian.Id,
					Date = DateTime.Now.AddDays(-1).Date,
					PlayedTime = 0
				};

				var profile = BungieApi.GetProfileResult(guardian.DestinyMembershipId, (int)guardian.DestinyMembershipType, DestinyComponentType.Profiles);
				if (profile.ErrorCode == 1 && profile != null)
				{
					foreach (var item in profile.Response.Profile.Data.CharacterIds)
					{
						var data = BungieApi.LoadCharacterActivityHistory((int)guardian.DestinyMembershipType, guardian.DestinyMembershipId, Convert.ToInt64(item));
						if (data.ErrorCode == 1 && data.Response.Activities != null)
						{
							for (int i = 0; i < data.Response.Activities.Length; i++)
							{
								if (data.Response.Activities[i].Period < DateTime.Now && data.Response.Activities[i].Period > DateTime.Now.AddDays(-2))
								{
									data.Response.Activities[i].Values.TryGetValue("activityDurationSeconds", out var value);
									stat.PlayedTime += (int)value.Basic.Value;
								}
							}
						}
						else
							logger.LogInformation($"Failed load data {guardian.Name} Response:\n ErrorCode: {data.ErrorCode}\n{data.ErrorStatus} - {data.Message}");
					}

				}
				else
					logger.LogInformation($"Bungie error on {guardian.Name} Response:\n ErrorCode: {profile.ErrorCode}\n{profile.ErrorStatus} - {profile.Message}");

				Db.Clan_Member_Stats.Add(stat);
				Db.SaveChanges();
			}
		}
	}
}
