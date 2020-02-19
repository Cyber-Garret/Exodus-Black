using Bot.Services.Data;

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
	public class XurLeave : IJob
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly GuildDataService guildData;

		public XurLeave(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<XurArrive>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			guildData = service.GetRequiredService<GuildDataService>();
		}
		public Task Execute(IJobExecutionContext context)
		{
			var embed = XurLeaveEmbed();

			Parallel.ForEach(discord.Guilds, async SocketGuild =>
			{
				try
				{
					var guild = guildData.GetGuildAccount(SocketGuild);

					if (guild.NotificationChannel == 0) return;

					await discord.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
				   .SendMessageAsync(text: guild.GlobalMention, embed: embed);
				}
				catch (Exception ex) { logger.LogError(ex, "XurLeave"); }
			});
			return Task.CompletedTask;
		}

		private Embed XurLeaveEmbed()
		{
			var embed = new EmbedBuilder
			{
				Title = "Внимание! Зур покинул солнечную систему.",
				Color = Color.Red,
				ThumbnailUrl = "https://www.bungie.net/common/destiny2_content/icons/5659e5fc95912c079962376dfe4504ab.png",
				Description = "Он просто испарился! :open_mouth: "
			};

			return embed.Build();
		}
	}
}
