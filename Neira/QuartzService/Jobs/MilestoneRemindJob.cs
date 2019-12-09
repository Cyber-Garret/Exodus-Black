using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Neira.Bot.Services;
using Neira.Database;

using Quartz;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.QuartzService
{
	public class MilestoneRemindJob : IJob
	{
		private readonly MilestoneService _milestone;
		public MilestoneRemindJob(IServiceProvider service)
		{
			_milestone = service.GetRequiredService<MilestoneService>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			var timer = DateTime.Now.AddMinutes(15);
			using var Db = new NeiraLinkContext();
			var query = Db.ActiveMilestones.Include(r => r.Milestone).Include(ac => ac.MilestoneUsers).OrderBy(o => o.DateExpire);

			if (query.Count() > 0)
			{
				Parallel.ForEach(query, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async item =>
				{
					if (timer.Date == item.DateExpire.Date && timer.Hour == item.DateExpire.Hour && timer.Minute == item.DateExpire.Minute && timer.Second < 10)
					{
						await _milestone.RaidNotificationAsync(item, MilestoneService.RemindType.ByTimer);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
