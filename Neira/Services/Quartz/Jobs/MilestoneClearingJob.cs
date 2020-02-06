using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neira.Database;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Services.Quartz
{
	public class MilestoneClearingJob : IJob
	{
		private readonly ILogger<MilestoneClearingJob> _logger;
		public MilestoneClearingJob(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneClearingJob>>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			using var Db = new NeiraLinkContext();
			var query = Db.ActiveMilestones.Include(r => r.Milestone).Include(ac => ac.MilestoneUsers).OrderBy(o => o.DateExpire);

			if (query.Count() > 0)
			{
				Parallel.ForEach(query, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async item =>
				{
					try
					{
						if ((item.DateExpire.AddMinutes(30) < DateTime.Now && item.Milestone.MaxSpace == item.MilestoneUsers.Count + 1) || item.DateExpire.AddHours(1) < DateTime.Now)
						{
							using var Db = new NeiraLinkContext();
							var milestone = Db.ActiveMilestones.First(m => m.MessageId == item.MessageId);
							Db.Remove(milestone);
							await Db.SaveChangesAsync();
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "MilestoneClearingJob");
					}

				});
			}
			return Task.CompletedTask;
		}
	}
}
