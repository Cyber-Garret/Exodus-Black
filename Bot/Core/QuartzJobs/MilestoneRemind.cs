using Bot.Core.Data;
using Bot.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace Bot.Core.QuartzJobs
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
			var timer = DateTime.Now.AddMinutes(15);

			var query = ActiveMilestoneData.GetAllMilestones();

			if (query.Count > 0)
			{
				Parallel.ForEach(query, async milestone =>
				{
					try
					{
						if (timer.Date == milestone.DateExpire.Date && timer.Hour == milestone.DateExpire.Hour && timer.Minute == milestone.DateExpire.Minute && timer.Second < 10)
						{
							await milestoneHandler.RaidNotificationAsync(milestone);
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
