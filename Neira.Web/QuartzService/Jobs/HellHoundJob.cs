using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Neira.Web.Database;
using Neira.Web.Models;

using Quartz;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neira.Web.QuartzService
{
	[DisallowConcurrentExecution]
	public class HellHoundJob : IJob
	{
		private readonly ILogger _logger;
		private readonly DiscordSocketClient _client;
		private readonly Config _config;
		public HellHoundJob(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<HellHoundJob>>();
			_client = service.GetRequiredService<DiscordSocketClient>();
			_config = service.GetRequiredService<IOptions<Config>>().Value;
		}
		public async Task Execute(IJobExecutionContext context)
		{
			try
			{
				using var Db = new NeiraLinkContext();
				//Stat counts
				var _online = 0;
				var _idle = 0;
				var _dnd = 0;
				var _inVoice = 0;
				var _inGame = 0;
				var _inDestiny2 = 0;
				var date = DateTime.Now;

				var guild = _client.GetGuild(_config.HellHoundDiscordServer);
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
					else if (user.Status == UserStatus.DoNotDisturb)
						Interlocked.Increment(ref _dnd);
				});

				var stat = new ADOnline
				{
					Online = _online,
					Idle = _idle,
					DnD = _dnd,
					InVoice = _inVoice,
					InGame = _inGame,
					InDestiny2 = _inDestiny2,
					Date = date
				};

				Db.ADOnlines.Add(stat);
				Db.SaveChanges();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Aggregate AD Stat");
			}
		}
	}
}
