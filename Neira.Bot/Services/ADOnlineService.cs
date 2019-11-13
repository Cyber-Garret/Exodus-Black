using Discord;
using Discord.WebSocket;

using Neira.Bot.Database;
using Neira.Bot.Helpers;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Neira.Bot.Services
{
	public class ADOnlineService
	{
		private readonly DiscordSocketClient Client;
		private Timer ADTimer;

		public ADOnlineService(DiscordSocketClient socketClient)
		{
			Client = socketClient;
		}

		public void Configure()
		{
			ADTimer = new Timer
			{
				Enabled = true,
				AutoReset = true,
				Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
			};
			ADTimer.Elapsed += ADTimer_ElapsedAsync;
		}

		private async void ADTimer_ElapsedAsync(object sender, ElapsedEventArgs e)
		{
			if (e.SignalTime.Minute == 00 && e.SignalTime.Second < 10)
				await AggregateStat();
		}

		private async Task AggregateStat()
		{
			try
			{
				using (var Db = new NeiraLinkContext())
				{
					//Stat counts
					var _online = 0;
					var _idle = 0;
					var _dnd = 0;
					var _inVoice = 0;
					var _afk = 0;
					var _inGame = 0;
					var _inDestiny2 = 0;
					var date = DateTime.Now;

					var guild = Client.GetGuild(Program.config.HellHoundDiscordServer);
					await guild.DownloadUsersAsync();

					var options = new ParallelOptions() { MaxDegreeOfParallelism = 10 };
					Parallel.ForEach(guild.Users, options, user =>
					{
						//Playing game?
						if (user.Activity != null)
							Interlocked.Increment(ref _inGame);
						//User playing Destiny 2?
						if (user.Activity?.Name == "Destiny 2")
							Interlocked.Increment(ref _inDestiny2);
						//Sit in voice channel of current guild?
						if (user.VoiceState.HasValue)
							Interlocked.Increment(ref _inVoice);
						//User current status
						if (user.Status == UserStatus.Online)
							Interlocked.Increment(ref _online);
						else if (user.Status == UserStatus.Idle)
							Interlocked.Increment(ref _idle);
						else if (user.Status == UserStatus.AFK)
							Interlocked.Increment(ref _afk);
						else if (user.Status == UserStatus.DoNotDisturb)
							Interlocked.Increment(ref _dnd);
					});

					var stat = new ADOnline
					{
						Online = _online,
						Idle = _idle,
						DnD = _dnd,
						InVoice = _inVoice,
						AFK = _afk,
						InGame = _inGame,
						InDestiny2 = _inDestiny2,
						Date = date
					};

					Db.ADOnlines.Add(stat);
					Db.SaveChanges();

				}
			}
			catch (Exception ex)
			{
				await Logger.LogFullException(new LogMessage(LogSeverity.Critical, "Aggregate AD Stat", ex.Message, ex));
				throw;
			}
		}
	}
}
