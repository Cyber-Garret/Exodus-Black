using Bot.Entity;
using Bot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot
{
	public class Program
	{
		public static void Main()
		{
			CreateHostBuilder().Build().Run();
		}

		public static IHostBuilder CreateHostBuilder() =>
			Host.CreateDefaultBuilder()
				.ConfigureServices((hostContext, services) =>
				{
					//load and map bot settings to class
					services.Configure<BotConfig>(options => hostContext.Configuration.GetSection("BotConfig").Bind(options));

					services.AddHostedService<Worker>();

					services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
					{
						ExclusiveBulkDelete = true,
						AlwaysDownloadUsers = true,
						LogLevel = LogSeverity.Warning,
						DefaultRetryMode = RetryMode.AlwaysRetry,
						MessageCacheSize = 1000
					}));
					services.AddSingleton<CommandService>();
					services.AddSingleton<DiscordLogging>();
					services.AddSingleton<CommandHandler>();
					services.AddSingleton<MilestoneEmoji>();
				});
	}
}
