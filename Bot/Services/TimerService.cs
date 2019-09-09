using Bot.Models.Db;
using Bot.Services.Bungie;

using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Bot.Services
{
	public class TimerService
	{
		private readonly DiscordSocketClient Client;
		private readonly MilestoneService Milestone;
		private readonly FailsafeContext Db;
		private Timer MainTimer;
		private Timer GameStatusTimer;
		private Timer ClanTimer;
		private Timer MemberTimer;
		private Timer MilestoneTimer;

		public TimerService(DiscordSocketClient socketClient, MilestoneService milestoneService, FailsafeContext failsafeContext)
		{
			Client = socketClient;
			Milestone = milestoneService;
			Db = failsafeContext;
		}

		public void Configure()
		{
			MainTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
			};
			MainTimer.Elapsed += MainTimer_Elapsed;

			GameStatusTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(30).TotalMilliseconds
			};
			GameStatusTimer.Elapsed += GameStatus_Elapsed;

			ClanTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromMinutes(15).TotalMilliseconds
			};
			ClanTimer.Elapsed += ClanTimer_Elapsed;

			MemberTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromHours(1).TotalMilliseconds
			};
			MemberTimer.Elapsed += MemberTimer_Elapsed;

			MilestoneTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
			};
			MilestoneTimer.Elapsed += MilestoneTimer_Elapsed;
		}

		#region Elapsed events
		private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			// If signal time equal Friday 20:00 we will send message Xur is arrived in game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Friday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurArrived();
			// If signal time equal Tuesday 20:00 we will send message Xur is leave game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Tuesday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurLeave();
		}
		private async void GameStatus_Elapsed(object sender, ElapsedEventArgs e)
		{
			string[] commands = new string[]
			{
				"!справка",
				"!зур",
				"!клан статус",
				"!респект",
				"!помощь",
				"!донат",
				"!клан",
				"!инфо дело",
				"!инфо свет",
				"!инфо морозники",
				"!инфо бешеный",
				"!катализатор свет",
				"!катализатор мида",
				"!катализатор бремя"
			};

			Random rand = new Random();
			int randomIndex = rand.Next(commands.Length);
			string text = commands[randomIndex];
			try
			{
				await Client.SetGameAsync(text, null, ActivityType.Playing);
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Change Bot Game Status", ex.Message, ex));
			}
		}
		private void ClanTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			ClanUpdater updater = new ClanUpdater();
			updater.UpdateAllClans();
			updater.ClanMemberCheck();
		}
		private void MemberTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MemberUpdater updater = new MemberUpdater();
			updater.UpdateMembersLastPlayedTime();
		}

		private void MilestoneTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_ = RaidRemainder();
		}
		#endregion

		#region Methods
		private async Task XurArrived()
		{
			#region Message
			var embed = new EmbedBuilder()
				.WithColor(Color.Orange)
				.WithTitle("Стражи! Зур прибыл в солнечную систему!")
				.WithThumbnailUrl("https://i.imgur.com/sFZZlwF.png")
				.WithDescription("Мои алгоритмы глобального позиционирования пока еще в разработке потому определить точное местоположение Зур-а я не могу.\n" +
					"[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
					"[Или тут](https://ftw.in/game/destiny-2/find-xur)")
				.WithFooter("Напоминаю! Зур покинет солнечную систему во вторник в 20:00 по МСК.");
			#endregion

			var guilds = await FailsafeDbOperations.GetAllGuildsAsync();

			foreach (var guild in guilds)
			{
				if (guild.NotificationChannel != 0)
				{
					try
					{
						await Client.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
					   .SendMessageAsync(text: guild.GlobalMention, embed: embed.Build());
					}
					catch (Exception ex)
					{
						await Logger.Log(new LogMessage(LogSeverity.Error, "XurArrived", ex.Message, ex));
					}

				}
			}
		}
		private async Task XurLeave()
		{
			#region Message
			var embed = new EmbedBuilder()
			   .WithColor(Color.Red)
			   .WithTitle("Внимание! Зур покинул солнечную систему.")
			   .WithThumbnailUrl("https://i.imgur.com/sFZZlwF.png")
			   .WithDescription("Он просто испарился! Как только он придёт я сообщу.")
			   .WithFooter("Напоминаю! В следующий раз Зур прибудет в пятницу в 20:00 по МСК.");
			#endregion

			var guilds = await FailsafeDbOperations.GetAllGuildsAsync();

			foreach (var guild in guilds)
			{
				if (guild.NotificationChannel != 0)
				{
					try
					{
						await Client.GetGuild(guild.Id).GetTextChannel(guild.NotificationChannel)
					   .SendMessageAsync(text: guild.GlobalMention, embed: embed.Build());
					}
					catch (Exception ex)
					{
						await Logger.Log(new LogMessage(LogSeverity.Error, "XurLeave", ex.Message, ex));
					}

				}
			}
		}
		private async Task RaidRemainder()
		{
			var timer = DateTime.Now.AddMinutes(15);

			var query = await Db.ActiveMilestones
				.Include(r => r.Milestone)
				.Include(ac => ac.MilestoneUsers)
				.OrderBy(o => o.DateExpire)
				.ToListAsync();

			if (query.Count > 0)
			{
				Parallel.ForEach(query, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async item =>
				  {
					  if (timer.Date == item.DateExpire.Date && timer.Hour == item.DateExpire.Hour && timer.Minute == item.DateExpire.Minute && timer.Second < 10)
					  {
						  //List with milestone leader and users who first click reaction
						  List<ulong> users = new List<ulong>();

						  //Get message by id from guild, in specific text channel, for read who clicked the represented reactions
						  var message = await Client.GetGuild(item.GuildId).GetTextChannel(item.TextChannelId).GetMessageAsync(item.MessageId) as IUserMessage;

						  if (item.MilestoneUsers.Count > 0)
						  {
							  foreach (var user in item.MilestoneUsers)
								  users.Add(user.UserId);

						  }
						  //Clean all reactions
						  await message.RemoveAllReactionsAsync();

						  //Add leader in list for friendly remainder in direct messaging
						  users.Add(item.Leader);
						  await Milestone.RaidNotificationAsync(users, item);

						  //Remove expired Milestone
						  Db.ActiveMilestones.Remove(item);
						  await Db.SaveChangesAsync();
					  }
				  });
			}
		}

		#endregion
	}
}
