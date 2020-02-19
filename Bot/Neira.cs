using Bot.Services;
using Bot.Services.Data;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
	public class Neira : BackgroundService
	{
		private readonly IConfiguration config;
		private readonly IServiceProvider service;
		private readonly ILogger<Neira> logger;
		private readonly DiscordSocketClient discord;
		private readonly ExoticDataService exoticData;
		private readonly CatalystDataService catalystData;
		private readonly GuildDataService guildData;
		private readonly MilestoneInfoDataService milestoneInfoData;
		private readonly MilestoneDataService milestoneData;

		public Neira(IServiceProvider service)
		{
			this.service = service;
			logger = service.GetRequiredService<ILogger<Neira>>();
			config = service.GetRequiredService<IConfiguration>();
			exoticData = service.GetRequiredService<ExoticDataService>();
			catalystData = service.GetRequiredService<CatalystDataService>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			guildData = service.GetRequiredService<GuildDataService>();
			milestoneInfoData = service.GetRequiredService<MilestoneInfoDataService>();
			milestoneData = service.GetRequiredService<MilestoneDataService>();
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			try
			{
				// Load data from local json files to in memory dictionaries.
				exoticData.LoadData();
				catalystData.LoadData();
				guildData.LoadData();
				milestoneInfoData.LoadData();
				milestoneData.LoadData();

				var token = config["Bot:Token"];

				service.GetRequiredService<LoggingService>().Configure();
				service.GetRequiredService<DiscordEventHandlerService>().Configure();
				await service.GetRequiredService<CommandHandlerService>().ConfigureAsync();

				await discord.LoginAsync(TokenType.Bot, token);
				await discord.StartAsync();
				await discord.SetStatusAsync(UserStatus.Online);
				await discord.SetGameAsync(@"http://neira.su/");

				await Task.Delay(-1, cancellationToken);

			}
			catch (TaskCanceledException) {/*We expect app throw TaskCanceledException if correct shutting down bot, anyway ignore this exception.*/ }
			catch (Exception ex)
			{
				logger.LogError(ex, "Neira Start");
				throw;
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await discord.SetStatusAsync(UserStatus.Offline);
			await discord.StopAsync();

			// save all guild accounts
			guildData.SaveAccounts();
			logger.LogInformation("Аккаунты успешно сохранены.");
			milestoneData.SaveMilestones();
			logger.LogInformation("Активности успешно сохранены.");
			
			discord.Dispose();
		}
	}
}
