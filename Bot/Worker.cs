using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly ExoticDataService exoticData;
		private readonly CatalystDataService catalystData;

		public Worker(IServiceProvider service, ILogger<Worker> logger)
		{
			_logger = logger;
			exoticData = service.GetRequiredService<ExoticDataService>();
			catalystData = service.GetRequiredService<CatalystDataService>();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			exoticData.LoadData();
			catalystData.LoadData();
			while (!stoppingToken.IsCancellationRequested)
			{
				//_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				exoticData.SearchExotic("мида");
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
