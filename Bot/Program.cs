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
						.AddSingleton<EmoteService>();
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
