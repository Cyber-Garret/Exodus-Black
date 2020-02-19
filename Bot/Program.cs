using Bot.Core.QuartzJobs;
using Bot.Services;
using Bot.Services.Data;

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;
using Quartz.Impl;
using Quartz.Spi;

using Serilog;
using Serilog.Core;

using System;

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
			var builtConfig = CreateConfigBuilder(args);

			var log = CreateSerilogLogger(builtConfig);

			var discordSeverity = (LogSeverity)Enum.Parse(typeof(LogSeverity), builtConfig["Bot:LogLevel"]);

			try
			{
				return Host.CreateDefaultBuilder(args)
					.ConfigureLogging(logger =>
					{
						logger.ClearProviders();
						logger.AddSerilog(logger: log, dispose: true);
					})
					.ConfigureServices((hostContext, services) =>
					{
						services.AddHostedService<Neira>();
						// Quartz services
						services.AddHostedService<Quartz>();
						services.AddSingleton<IJobFactory, SingletonJobFactory>();
						services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
						// File storages
						services.AddSingleton<ExoticDataService>();
						services.AddSingleton<CatalystDataService>();
						services.AddSingleton<GuildDataService>();
						// bot services
						services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
						{
							ExclusiveBulkDelete = true,
							AlwaysDownloadUsers = true,
							LogLevel = discordSeverity,
							DefaultRetryMode = RetryMode.AlwaysRetry,
							MessageCacheSize = 300
						}))
						.AddSingleton<CommandService>()
						.AddSingleton<InteractiveService>()
						.AddSingleton<LoggingService>()
						.AddSingleton<DiscordEventHandlerService>()
						.AddSingleton<CommandHandlerService>()
						.AddSingleton<EmoteService>()
						.AddSingleton<SelfRoleService>();
						// Quartz jobs
						services.AddSingleton<XurArrive>();
						services.AddSingleton<XurLeave>();
						services.AddSingleton<MilestoneRemind>();
						// Quartz triggers
						var hour = hostContext.Configuration.GetSection("Bot:XurHour").Get<int>();
						services.AddSingleton(new JobSchedule(typeof(XurArrive), $"0 0 {hour} ? * FRI")); // run every Friday in 20:00
						services.AddSingleton(new JobSchedule(typeof(XurLeave), $"0 0 {hour} ? * TUE")); // run every Tuesday in 20:00

						services.AddSingleton(new JobSchedule(typeof(MilestoneRemind), "0/10 * * * * ?")); // run every 10 seconds.
					})
					.ConfigureAppConfiguration((hostingContext, config) =>
					{
						config.AddConfiguration(builtConfig);
					});
			}
			catch (Exception ex)
			{
				log.Fatal(ex, "Host builder error");
				throw;
			}
			finally
			{
				log.Dispose();
			}
		}

		/// <summary>
		/// Build configuration with appsettings.json and command line args
		/// </summary>
		/// <param name="args">cmd arguments</param>
		private static IConfigurationRoot CreateConfigBuilder(string[] args)
		{
			return new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddCommandLine(args)
			.Build();
		}

		/// <summary>
		/// Create serilog log configuration and return Logger class
		/// </summary>
		private static Logger CreateSerilogLogger(IConfigurationRoot configuration)
		{
			// create logger with console output by default
			var logger = new LoggerConfiguration()
				.WriteTo.Console();
			// get path for logging in file from appsettings.json
			var logPath = configuration["Logging:FilePath"];

			// check if filepath for logging presented
			if (!string.IsNullOrWhiteSpace(logPath))
			{
				logger.WriteTo.File(logPath);
				return logger.CreateLogger();
			}
			else
			{
				return logger.CreateLogger();
			}
		}
	}
}
