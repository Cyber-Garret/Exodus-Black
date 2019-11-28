using Microsoft.Extensions.Logging;

using Neira.API.Bungie;
using Neira.Web.Database;

using Quartz;

using System;
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
			logger.LogInformation("GrabGuardianPlayedTime start working at {RequestTime}", DateTime.Now);

			var yesterday = DateTime.Now.AddDays(-1);
			using var Db = new NeiraLinkContext();
			var guardians = Db.Clan_Members.ToList();

			var BungieApi = new BungieApi();

			Parallel.ForEach(guardians, new ParallelOptions { MaxDegreeOfParallelism = 10 }, guardian =>
			{
				using var DbContext = new NeiraLinkContext();
				if (StatExist(guardian.Id, yesterday)) return;

				var profile = BungieApi.GetProfileResult(guardian.DestinyMembershipId, (int)guardian.DestinyMembershipType, DestinyComponentType.Profiles);
				var BungieResponseCode = 0;
				if (profile.ErrorCode == 1 && profile != null)
				{
					var stat = new Clan_Member_Stat
					{
						MemberId = guardian.Id,
						Date = yesterday.Date,
						PlayedTime = 0
					};

					foreach (var item in profile.Response.Profile.Data.CharacterIds)
					{
						var data = BungieApi.LoadCharacterActivityHistory((int)guardian.DestinyMembershipType, guardian.DestinyMembershipId, Convert.ToInt64(item));
						//ErrorCode 1 means all ok
						if (data.ErrorCode == 1 && data.Response.Activities != null)
						{
							for (int i = 0; i < data.Response.Activities.Length; i++)
							{
								if (data.Response.Activities[i].Period.Date == stat.Date.Date)
								{
									data.Response.Activities[i].Values.TryGetValue("activityDurationSeconds", out var value);
									stat.PlayedTime += (int)value.Basic.Value;
								}
							}
						}
						BungieResponseCode = (int)data.ErrorCode;
					}

					guardian.ErrorCode = BungieResponseCode;

					if (BungieResponseCode != 1665)
						DbContext.Clan_Member_Stats.Add(stat);

					DbContext.Clan_Members.Update(guardian);
					DbContext.SaveChanges();

				}
				else
					logger.LogInformation($"Bungie error on {guardian.Name} Response:\n ErrorCode: {profile.ErrorCode}\n{profile.ErrorStatus} - {profile.Message}");
			});
			logger.LogInformation("GrabGuardianPlayedTime done work at {responseTime}", DateTime.Now);
		}

		private bool StatExist(int GuardianId, DateTime date)
		{
			using var Db = new NeiraLinkContext();
			return Db.Clan_Member_Stats.Any(m => m.MemberId == GuardianId && m.Date.Date == date.Date);
		}
	}
}
