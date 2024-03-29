﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Properties;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;

namespace Failsafe.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class XurLeave : IJob
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;

		public XurLeave(IServiceProvider service)
		{
			logger = service.GetRequiredService<ILogger<XurArrive>>();
			discord = service.GetRequiredService<DiscordSocketClient>();
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
				   .SendMessageAsync(embed: XurLeaveEmbed(guild.Language).Build());
				}
				catch (Exception ex) { logger.LogError(ex, "XurLeave"); }
			});
			return Task.CompletedTask;
		}

		private static EmbedBuilder XurLeaveEmbed(CultureInfo culture)
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
