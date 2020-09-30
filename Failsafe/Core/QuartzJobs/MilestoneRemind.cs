using Failsafe.Core.Data;
using Failsafe.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace Failsafe.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class MilestoneRemind : IJob
	{
		private readonly ILogger _logger;
		private readonly MilestoneService milestoneHandler;
		public MilestoneRemind(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneRemind>>();
			milestoneHandler = service.GetRequiredService<MilestoneService>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			var timer = DateTime.UtcNow.AddMinutes(15);

			var query = ActiveMilestoneData.GetAllMilestones();

			if (query.Count > 0)
			{
				Parallel.ForEach(query, async milestone =>
				{
					try
					{
						var guild = GuildData.GetGuildAccount(milestone.GuildId);
						var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
						var now = TimeZoneInfo.ConvertTimeFromUtc(timer, guildTimeZone);

						if (now.Date == milestone.DateExpire.Date && now.Hour == milestone.DateExpire.Hour && now.Minute == milestone.DateExpire.Minute && now.Second < 10)
						{
							await milestoneHandler.MilestoneNotificationAsync(milestone);
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "MilestoneRemindJob");
					}

				});
			}
			return Task.CompletedTask;
		}
	}
}
