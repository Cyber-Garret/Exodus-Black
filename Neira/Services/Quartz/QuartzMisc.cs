using Microsoft.Extensions.DependencyInjection;

using Quartz;
using Quartz.Spi;

using System;

namespace Neira.Services.Quartz
{
	public class JobSchedule
	{
		public JobSchedule(Type jobType, string cronExpression)
		{
			JobType = jobType;
			CronExpression = cronExpression;
		}

		public Type JobType { get; }
		public string CronExpression { get; }
	}

	public class SingletonJobFactory : IJobFactory
	{
		private readonly IServiceProvider _serviceProvider;
		public SingletonJobFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
		{
			return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
		}

		public void ReturnJob(IJob job) { }
	}
}
