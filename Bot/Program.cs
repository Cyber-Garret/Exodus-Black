using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Core;
using Serilog;

namespace Bot
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var builtConfig = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddCommandLine(args)
			.Build();

			var log = new LoggerConfiguration()
				.WriteTo.Console()
				.CreateLogger();
			//.WriteTo.File(builtConfig["Logging:FilePath"])

			try
			{
				return Host.CreateDefaultBuilder(args)
					.ConfigureLogging(logger =>
					{
						logger.ClearProviders();
						logger.AddSerilog(log);
					})
					.ConfigureServices((hostContext, services) =>
					{
						services.AddHostedService<Worker>();
						// File storages
						services.AddSingleton<ExoticDataService>();
						services.AddSingleton<CatalystDataService>();
					})
					.ConfigureAppConfiguration((hostingContext, config) =>
					{
						config.AddConfiguration(builtConfig);
					});
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host builder error");

				throw;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
