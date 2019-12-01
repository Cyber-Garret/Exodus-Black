using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Neira.Bot.Services;
using Neira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neira.Bot
{
	public class BotHostedService : IHostedService
	{
		private readonly DiscordSocketClient _discord;
		private readonly IServiceProvider _service;
		private readonly Config _config;
		public BotHostedService(IServiceProvider service)
		{
			_discord = service.GetRequiredService<DiscordSocketClient>();
			_config = service.GetRequiredService<IOptions<Config>>().Value;
			_service = service;
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_service.GetRequiredService<LoggingService>();
			_service.GetRequiredService<GuildEventHandlerService>().Configure();
			await _service.GetRequiredService<CommandHandlerService>().ConfigureAsync();


			await _discord.LoginAsync(TokenType.Bot, _config.BotToken);
			await _discord.StartAsync();
			await _discord.SetStatusAsync(UserStatus.Online);
			await _discord.SetGameAsync(@"http://neira.su/");

		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await _discord.SetStatusAsync(UserStatus.Offline);
			await _discord.LogoutAsync();
		}
	}
}
