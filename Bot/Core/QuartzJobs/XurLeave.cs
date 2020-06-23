using Bot.Core.Data;
using Bot.Properties;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class XurLeave : IJob
	{
		private readonly ILogger logger;
		private readonly DiscordShardedClient discord;

		public XurLeave(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<XurArrive>>();
			discord = service.GetRequiredService<DiscordShardedClient>();
		}
		public Task Execute(IJobExecutionContext context)
		{

			Parallel.ForEach(discord.Guilds, async SocketGuild =>
			{
				try
				{
					var guild = GuildData.GetGuildAccount(SocketGuild);

					if (guild.NotificationChannel == 0) return;

					await discord.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
				   .SendMessageAsync(text: guild.GlobalMention, embed: XurLeaveEmbed(guild.Language).Build());
				}
				catch (Exception ex) { logger.LogError(ex, "XurLeave"); }
			});
			return Task.CompletedTask;
		}

		private EmbedBuilder XurLeaveEmbed(CultureInfo culture)
		{
			Thread.CurrentThread.CurrentUICulture = culture;

			var embed = new EmbedBuilder
			{
				Title = Resources.XurLeaveEmbTitle,
				Color = Color.Red,
				ThumbnailUrl = "https://www.bungie.net/common/destiny2_content/icons/5659e5fc95912c079962376dfe4504ab.png",
				Description = Resources.XurLeaveEmbDesc
			};

			return embed;
		}
	}
}
