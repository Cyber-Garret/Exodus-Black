using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neira.Bot.Helpers;
using Neira.Database;

using Quartz;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.QuartzService
{
	[DisallowConcurrentExecution]
	public class XurLeaveJob : IJob
	{
		private readonly ILogger _logger;
		private readonly DiscordSocketClient _discord;
		public XurLeaveJob(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<XurArrivedJob>>();
			_discord = service.GetRequiredService<DiscordSocketClient>();
		}

		public Task Execute(IJobExecutionContext context)
		{
			Parallel.ForEach(_discord.Guilds, async SocketGuild =>
			{
				try
				{
					using var Db = new NeiraLinkContext();

					var guild = Db.Guilds.FirstOrDefault(g => g.Id == SocketGuild.Id);
					if (guild == null || guild.NotificationChannel == 0) return;

					await _discord.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
				   .SendMessageAsync(text: guild.GlobalMention, embed: EmbedsHelper.XurLeave());
				}
				catch (Exception ex) { _logger.LogError(ex, "XurLeave"); }
			});
			return Task.CompletedTask;
		}
	}
}
