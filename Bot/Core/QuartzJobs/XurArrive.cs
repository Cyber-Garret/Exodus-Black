using Bot.Core.Data;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace Bot.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class XurArrive : IJob
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;

		public XurArrive(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<XurArrive>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			var embed = XurArriveEmbed();

			Parallel.ForEach(discord.Guilds, async SocketGuild =>
			{
				try
				{
					var guild = GuildData.GetGuildAccount(SocketGuild);

					if (guild.NotificationChannel == 0) return;

					await discord.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
				   .SendMessageAsync(text: guild.GlobalMention, embed: embed);
				}
				catch (Exception ex) { logger.LogError(ex, "XurArrived"); }
			});
			return Task.CompletedTask;
		}

		private Embed XurArriveEmbed()
		{
			var embed = new EmbedBuilder
			{
				Title = "Стражи! Зур прибыл в солнечную систему!",
				Color = Color.Gold,
				ThumbnailUrl = "https://www.bungie.net/common/destiny2_content/icons/5659e5fc95912c079962376dfe4504ab.png",
				Description =
				"Определить точное местоположение Зур-а я не могу.\n" +
				"[Тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
				"[Или тут](https://ftw.in/game/destiny-2/find-xur)"
			};

			return embed.Build();
		}
	}
}
