using System;
using System.Timers;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Core;
using System.Linq;
using Core.Models.Db;

namespace DiscordBot.Services
{
	public class TimerService
	{
		#region Private fields
		private readonly DiscordShardedClient _client = Program.Client;
		private Timer _timer;
		private Timer _statusTimer;
		#endregion

		public void Configure()
		{
			// Initialize timer for 10 sec.
			_timer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
			_timer.Elapsed += MainTimer;
			_timer.AutoReset = true;
			_timer.Enabled = true;

			// Initialize timer for 15 Min.
			_statusTimer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
			_statusTimer.Elapsed += ChangeGameStatus;
			_statusTimer.AutoReset = true;
			_statusTimer.Enabled = true;
		}

		private async void ChangeGameStatus(object sender, ElapsedEventArgs e)
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
				await _client.SetGameAsync(text, null, ActivityType.Playing);
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"ChangeGameStatus Method - {ex.Source}", ex.Message, ex.InnerException));
			}
		}

		private async void MainTimer(object sender, ElapsedEventArgs e)
		{
			// If signal time equal Friday 20:00 we will send message Xur is arrived in game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Friday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurArrived();
			// If signal time equal Tuesday 20:00 we will send message Xur is leave game.
			if (e.SignalTime.DayOfWeek == DayOfWeek.Tuesday && e.SignalTime.Hour == 20 && e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await XurLeave();
			await RaidRemainder(e);
		}

		private async Task XurArrived()
		{
			#region Message
			var embed = new EmbedBuilder()
				.WithColor(Color.Orange)
				.WithTitle("Стражи! Зур прибыл в солнечную систему!")
				.WithThumbnailUrl("http://neira.link/img/Xur_emblem.png")
				.WithDescription("Нажмите на заголовок сообщения чтобы узнать точное местоположение посланника Зура.")
				.WithUrl("https://whereisxur.com/")
				.WithFooter("Напоминаю! Зур покинет солнечную систему во вторник в 20:00 по МСК.")
				.WithCurrentTimestamp();
			#endregion

			var guilds = await FailsafeDbOperations.GetAllGuildsAsync();

			foreach (var guild in guilds)
			{
				if (guild.EnableNotification == true)
				{
					try
					{
						await _client.GetGuild(guild.ID).GetTextChannel(guild.NotificationChannel)
					   .SendMessageAsync(null, false, embed.Build());
					}
					catch (Exception ex)
					{
						LogMessage message = new LogMessage(LogSeverity.Error, $"Xur Arrived Method - {ex.Source}", ex.Message, ex.InnerException);
						await Logger.Log(message);
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
			   .WithThumbnailUrl("http://neira.link/img/Xur_emblem.png")
			   .WithDescription("Он просто испарился! Как только он придёт я сообщу.")
			   .WithFooter("Напоминаю! В следующий раз Зур прибудет в пятницу в 20:00 по МСК.")
			   .WithCurrentTimestamp();
			#endregion

			var guilds = await FailsafeDbOperations.GetAllGuildsAsync();

			foreach (var guild in guilds)
			{
				if (guild.EnableNotification == true)
				{
					try
					{
						await _client.GetGuild(guild.ID).GetTextChannel(guild.NotificationChannel)
					   .SendMessageAsync(null, false, embed.Build());
					}
					catch (Exception ex)
					{
						LogMessage message = new LogMessage(LogSeverity.Error, $"Xur Leave Method - {ex.Source}", ex.Message, ex.InnerException);
						await Logger.Log(message);
					}

				}
			}
		}

		private async Task RaidRemainder(ElapsedEventArgs elapsed)
		{
			using (FailsafeContext Db = new FailsafeContext())
			{
				foreach (var item in Db.ActiveRaids.ToList())
				{
					if (elapsed.SignalTime.Date == item.DateExpire.Date
						&& elapsed.SignalTime.Hour == item.DateExpire.Hour
						&& elapsed.SignalTime.AddMinutes(15).Minute == item.DateExpire.Minute
						&& elapsed.SignalTime.Second < 10)
					{
						await RaidNotificationAsync(item.User1, item);
						await RaidNotificationAsync(item.User2, item);
						await RaidNotificationAsync(item.User3, item);
						await RaidNotificationAsync(item.User4, item);
						await RaidNotificationAsync(item.User5, item);
						await RaidNotificationAsync(item.User6, item);
					}
				}
			}
		}

		private async Task RaidNotificationAsync(ulong userId, ActiveRaid raid)
		{
			if (userId != 0)
			{
				try
				{
					SocketUser User = Program.Client.GetUser(userId);
					IDMChannel Dm = await User.GetOrCreateDMChannelAsync();

					#region Message
					EmbedBuilder embed = new EmbedBuilder();
					embed.WithAuthor($"Доброго времени суток, {User.Username}");
					embed.WithTitle($"Хочу вам напомнить, что у вас через 15 минут начнется рейд.");
					embed.WithColor(Color.DarkMagenta);
					embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
					embed.WithDescription(raid.Description);
					embed.WithFooter($"Рейд: {raid.Name}. Сервер: {raid.Guild}");
					#endregion

					await Dm.SendMessageAsync(embed: embed.Build());
				}
				catch (Exception ex)
				{
					await Logger.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
				}

			}

		}
	}
}
