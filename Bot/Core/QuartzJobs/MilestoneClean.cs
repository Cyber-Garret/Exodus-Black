using Bot.Core.Data;

using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace Bot.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class MilestoneClean : IJob
	{
		private readonly ILogger logger;

		public MilestoneClean(ILogger<MilestoneClean> logger) => this.logger = logger;

		public Task Execute(IJobExecutionContext context)
		{
			logger.LogInformation("Milestone cleaning job start work");
			var query = ActiveMilestoneData.GetAllMilestones();

			if (query.Count > 0)
			{
				Parallel.ForEach(query, milestone =>
				{
					try
					{
						var guild = GuildData.GetGuildAccount(milestone.GuildId);
						var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
						var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone);
						var timer = milestone.DateExpire.AddHours(1);
						if (now > timer)
						{
							ActiveMilestoneData.RemoveMilestone(milestone.MessageId);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Milestone cleaning job");
					}

				});
			}
			return Task.CompletedTask;
		}
	}
}
