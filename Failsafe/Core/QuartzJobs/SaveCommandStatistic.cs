using Failsafe.Core.Data;

using Quartz;

using System.Threading.Tasks;

namespace Failsafe.Core.QuartzJobs
{
    [DisallowConcurrentExecution]
    public class SaveCommandStatistic : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            CommandStatisticData.SaveStatByDay();
            return Task.CompletedTask;
        }
    }
}
