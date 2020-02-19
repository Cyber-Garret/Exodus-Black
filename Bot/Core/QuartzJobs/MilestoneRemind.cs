using Bot.Services;
using Bot.Services.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class MilestoneRemind : IJob
	{
		private readonly ILogger _logger;
		private readonly MilestoneDataService milestoneData;
		private readonly MilestoneHandlerService milestoneHandler;
		public MilestoneRemind(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneRemind>>();
			milestoneData = service.GetRequiredService<MilestoneDataService>();
			milestoneHandler = service.GetRequiredService<MilestoneHandlerService>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			var timer = DateTime.Now.AddMinutes(15);

			var query = milestoneData.GetAllMilestones();

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
