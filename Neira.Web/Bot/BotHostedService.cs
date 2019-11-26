using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neira.Web.Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neira.Web.Bot
{
	public class BotHostedService : IHostedService
	{
		private readonly DiscordSocketClient _discord;
		private readonly IServiceProvider _service;
		public BotHostedService(IServiceProvider service)
		{
			_discord = service.GetRequiredService<DiscordSocketClient>();
			_service = service;
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_service.GetRequiredService<LoggingService>();
			//_service.GetRequiredService<DiscordEventHandlerService>().Configure();
			//await _service.GetRequiredService<CommandHandlerService>().ConfigureAsync();
			//_service.GetRequiredService<XurService>().Configure();
			//_service.GetRequiredService<ADOnlineService>().Configure();


			await _discord.LoginAsync(TokenType.Bot, "NTMyMjI1MTg5ODgzMjgxNDA4.Xd07tg.HZD1tbJkwXhcJ5QShrQpl9xx4fY", true);
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
