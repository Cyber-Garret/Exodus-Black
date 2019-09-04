using Bot.Models.Db.Destiny2;
using Bot.Services.Bungie;

using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Bot.Services
{
	public class TimerService
	{
		private readonly DiscordSocketClient Client;
		private readonly MilestoneService Milestone;
		private Timer MainTimer;
		private Timer GameStatusTimer;
		private Timer ClanTimer;
		private Timer MemberTimer;

		public TimerService(DiscordSocketClient socketClient, MilestoneService milestoneService)
		{
			Client = socketClient;
			Milestone = milestoneService;
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
		}

		#region Elapsed events
		private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_ = RaidRemainder();
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

			var timer = DateTime.Now.AddMinutes(1);

			using (FailsafeContext Db = new FailsafeContext())
			{
				var query = await Db.ActiveMilestones.Include(r => r.Milestone).Where(d => d.DateExpire > DateTime.Now).OrderBy(o => o.DateExpire).ToListAsync();
				if (query.Count > 0)
				{
					foreach (var item in query)
					{
						if (timer.Date == item.DateExpire.Date && timer.Hour == item.DateExpire.Hour && timer.Minute == item.DateExpire.Minute && timer.Second < 10)
						{
							List<ulong> users = new List<ulong>();
							//TODO: LoadMessage Get first User of specific free places
							var message = await Client.GetGuild(item.GuildId).GetTextChannel(item.TextChannelId).GetMessageAsync(item.MessageId) as IUserMessage;
							users.Add(item.Leader);
							for (int i = 0; i < item.Places; i++)
							{
								var user = await message.GetReactionUsersAsync(Global.ReactPlaceNumber[$"{i + 2}"], 1).FlattenAsync();
								users.Add(user.FirstOrDefault().Id);
							}
							//TODO: modify message
							await message.ModifyAsync(m =>
							{
								m.Content = FailsafeDbOperations.GetGuildAccountAsync(item.GuildId).Result.GlobalMention;
								m.Embed = await Milestone.
								});
							await RaidNotificationAsync(users, item);
							//Remove expired Milestone
							Db.ActiveMilestones.Remove(item);
							await Db.SaveChangesAsync();
						}
					}
				}
			}
		}
		private async Task RaidNotificationAsync(List<ulong> userIds, ActiveMilestone milestone)
		{
			foreach (var item in userIds)
			{
				if (item != 0 || item != Client.CurrentUser.Id)
				{
					try
					{
						var User = Client.GetUser(item);
						var Guild = Client.GetGuild(milestone.GuildId);
						IDMChannel Dm = await User.GetOrCreateDMChannelAsync();

						#region Message
						EmbedBuilder embed = new EmbedBuilder();
						embed.WithAuthor($"Доброго времени суток, {User.Username}");
						embed.WithTitle($"Хочу вам напомнить, что у вас через 15 минут начнется {milestone.Milestone.Type.ToLower()}.");
						embed.WithColor(Color.DarkMagenta);
						if (milestone.Milestone.PreviewDesc != null)
							embed.WithDescription(milestone.Milestone.PreviewDesc);
						embed.WithThumbnailUrl(milestone.Milestone.Icon);
						if (milestone.Memo != null)
							embed.AddField("Заметка от лидера:", milestone.Memo);
						embed.WithFooter($"{milestone.Milestone.Type}: {milestone.Milestone.Name}. Сервер: {Guild.Name}");
						#endregion

						await Dm.SendMessageAsync(embed: embed.Build());
					}
					catch (Exception ex)
					{
						await Logger.Log(new LogMessage(LogSeverity.Error, "RaidNotification", ex.Message, ex));
					}

				}
			}


		}
		#endregion











	}
}
