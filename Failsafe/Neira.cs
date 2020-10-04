using Discord;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Properties;
using Failsafe.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Failsafe
{
	public class Neira : BackgroundService
	{
		private readonly IConfiguration config;
		private readonly IServiceProvider service;
		private readonly ILogger<Neira> logger;
		private readonly DiscordSocketClient discord;

		public Neira(IServiceProvider service)
		{
			this.service = service;
			logger = service.GetRequiredService<ILogger<Neira>>();
			config = service.GetRequiredService<IConfiguration>();
			discord = service.GetRequiredService<DiscordSocketClient>();
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			try
			{
				var token = config["Bot:Token"];
				service.GetRequiredService<DiscordEventHandlerService>().Configure();
				await service.GetRequiredService<CommandHandlerService>().InstallCommandsAsync();

				await discord.LoginAsync(TokenType.Bot, token);
				await discord.StartAsync();
				await discord.SetStatusAsync(UserStatus.Online);
				await discord.SetGameAsync(Resources.NeiraWebSite);

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

			// save all data to hdd
			GuildData.SaveAccounts();
			ActiveMilestoneData.SaveMilestones();

			discord.Dispose();
		}
	}
}
