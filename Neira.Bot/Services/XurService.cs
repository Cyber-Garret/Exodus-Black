using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Neira.Bot.Database;
using Neira.Bot.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Neira.Bot.Services
{
	public class XurService
	{
		private readonly DiscordSocketClient Client;
		private Timer XurTimer;

		public XurService(DiscordSocketClient socketClient)
		{
			Client = socketClient;
		}

		public void Configure()
		{
			XurTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
			};
			XurTimer.Elapsed += MainTimer_Elapsed;
		}
		private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// If signal time equal Friday 20:00 we will send message Xur is arrived in game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Friday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurArrived();
			// If signal time equal Tuesday 20:00 we will send message Xur is leave game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Tuesday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurLeave();
		}

		private async Task XurArrived()
		{
			using (var Db = new NeiraLinkContext())
			{
				foreach (var guild in Db.Guilds)
				{
					if (guild.NotificationChannel != 0)
					{
						try
						{
							await Client.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
						   .SendMessageAsync(text: guild.GlobalMention, embed: EmbedsHelper.XurArrived());
						}
						catch (Exception ex)
						{
							await Logger.Log(new LogMessage(LogSeverity.Error, "XurArrived", ex.Message, ex));
						}

					}
				}
			}
		}
		private async Task XurLeave()
		{
			using (var Db = new NeiraLinkContext())
			{
				foreach (var guild in Db.Guilds)
				{
					if (guild.NotificationChannel != 0)
					{
						try
						{
							await Client.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
						   .SendMessageAsync(text: guild.GlobalMention, embed: EmbedsHelper.XurLeave());
						}
						catch (Exception ex)
						{
							await Logger.Log(new LogMessage(LogSeverity.Error, "XurLeave", ex.Message, ex));
						}

					}
				}
			}
		}
	}
}
