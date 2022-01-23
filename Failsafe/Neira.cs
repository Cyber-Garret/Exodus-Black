using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Properties;
using Failsafe.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Failsafe
{
	public class Neira : BackgroundService
	{

		private readonly IServiceProvider _service;
		private readonly IConfiguration _config;
		private readonly ILogger<Neira> _logger;
		private readonly DiscordSocketClient _discord;
		private readonly DiscordRestClient _discordRest;

		public Neira(IServiceProvider service, IConfiguration config, ILogger<Neira> logger, DiscordSocketClient discord, DiscordRestClient discordRest)
		{
			_service = service;
			_config = config;
			_logger = logger;
			_discord = discord;
			_discordRest = discordRest;
		}


		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation($"Discord.NET version:{DiscordConfig.Version}");
				var token = _config["Bot:Token"];
				_service.GetRequiredService<DiscordEventHandlerService>().Configure();
				await _service.GetRequiredService<CommandHandlerService>().InstallCommandsAsync();

				//Load socket client
				await _discord.LoginAsync(TokenType.Bot, token);
				await _discord.StartAsync();
				await _discord.SetStatusAsync(UserStatus.Online);
				await _discord.SetGameAsync(Resources.NeiraWebSite);

				//Load rest client
				await _discordRest.LoginAsync(TokenType.Bot, token);


				await Task.Delay(-1, cancellationToken);

			}
			catch (TaskCanceledException) {/*We expect app throw TaskCanceledException if correct shutting down bot, anyway ignore this exception.*/ }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Neira Start");
				throw;
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await _discord.SetStatusAsync(UserStatus.Offline);
			await _discord.LogoutAsync();
			await _discordRest.LogoutAsync();

			// save all data to hdd
			GuildData.SaveAccounts();
			ActiveMilestoneData.SaveMilestones();

			_discord.Dispose();
		}
	}
}
