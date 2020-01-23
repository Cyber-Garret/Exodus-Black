using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Entity;
using Bot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bot
{
	public class Worker : BackgroundService
	{
		private readonly IServiceProvider _service;
		private readonly ILogger<Worker> _logger;
		private readonly DiscordSocketClient _discord;
		private readonly BotConfig _config;

		public Worker(IServiceProvider service)
		{
			_service = service;
			_logger = service.GetRequiredService<ILogger<Worker>>();
			_discord = service.GetRequiredService<DiscordSocketClient>();
			_config = service.GetRequiredService<IOptions<BotConfig>>().Value;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//Initialize service
			_service.GetRequiredService<DiscordLogging>().Configure();
			//_service.GetRequiredService<GuildEventHandlerService>().Configure();
			//await _service.GetRequiredService<CommandHandlerService>().ConfigureAsync();

			await _discord.LoginAsync(TokenType.Bot, _config.Token);
			await _discord.StartAsync();
			await _discord.SetStatusAsync(UserStatus.Online);
			await Task.Delay(-1, stoppingToken);

			if (stoppingToken.IsCancellationRequested)
			{
				await _discord.SetStatusAsync(UserStatus.Offline);
				await _discord.LogoutAsync();
			}
		}
	}
}
