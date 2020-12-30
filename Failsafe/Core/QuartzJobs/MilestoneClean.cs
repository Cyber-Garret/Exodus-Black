using Failsafe.Core.Data;

using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Failsafe.Core.QuartzJobs
{
	[DisallowConcurrentExecution]
	public class MilestoneClean : IJob
	{
		private readonly ILogger<MilestoneClean> _logger;
		private readonly DiscordSocketClient _discord;

		public MilestoneClean(ILogger<MilestoneClean> logger, DiscordSocketClient discord)
		{
			_logger = logger;
			_discord = discord;
		}

		public Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("Milestone cleaning job start work");
			var query = ActiveMilestoneData.GetAllMilestones();

			if (query.Count > 0)
			{
				Parallel.ForEach(query, async milestone =>
				{
					try
					{
						var guild = GuildData.GetGuildAccount(milestone.GuildId);
						var guildTimeZone = TimeZoneInfo.FindSystemTimeZoneById(guild.TimeZone);
						var now = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guildTimeZone), guildTimeZone.BaseUtcOffset);
						var timer = milestone.DateExpire.AddHours(1);

						if (now < timer) return;

						ActiveMilestoneData.RemoveMilestone(milestone.MessageId);
						var message = await _discord.GetGuild(milestone.GuildId).GetTextChannel(milestone.ChannelId)
							.GetMessageAsync(milestone.MessageId);

						if (message != null)
							await message.DeleteAsync();


					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Milestone cleaning job");
					}

				});
			}
			return Task.CompletedTask;
		}
	}
}
