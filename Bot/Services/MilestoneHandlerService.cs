using Bot.Models;
using Bot.Services.Data;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class MilestoneHandlerService
	{
		private readonly ILogger _logger;
		private readonly DiscordSocketClient _discord;
		private readonly EmoteService _emote;
		private readonly MilestoneDataService milestoneData;

		public MilestoneHandlerService(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneHandlerService>>();
			_discord = service.GetRequiredService<DiscordSocketClient>();
			_emote = service.GetRequiredService<EmoteService>();
			milestoneData = service.GetRequiredService<MilestoneDataService>();
		}

		public async Task MilestoneReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = milestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId);

					if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count + 1 < milestone.MilestoneInfo.MaxSpace)
					{
						var user = new MilestoneUser
						{
							UserId = reaction.UserId
						};
						milestone.MilestoneUsers.Add(user);
						milestoneData.SaveMilestones(milestone.MessageId);

						HandleReaction(msg, milestone);
					}
					else
					{
						var user = _discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Added in Milestone");
			}
		}

		public async Task MilestoneReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = milestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId);

					if (reaction.UserId != milestone.Leader && UserExist)
					{
						var user = new MilestoneUser { UserId = reaction.UserId };
						milestone.MilestoneUsers.Remove(user);
						milestoneData.SaveMilestones(milestone.MessageId);
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = _discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}

			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Removed in Milestone");
			}
		}

		private async void HandleReaction(IUserMessage message, Milestone milestone)
		{
			var newEmbed = MilestoneEmbed(milestone);
			if (newEmbed.Length != 0)
				await message.ModifyAsync(m => m.Embed = newEmbed);
		}

		public async Task RaidNotificationAsync(Milestone milestone)
		{
			try
			{
				var Guild = _discord.GetGuild(milestone.GuildId);
				var RemindEmbed = MilestoneRemindEmbed(milestone);

				var Leader = _discord.GetUser(milestone.Leader);
				var LeaderDM = await Leader.GetOrCreateDMChannelAsync();

				await LeaderDM.SendMessageAsync(embed: RemindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{
						var LoadedUser = _discord.GetUser(user.UserId);

						var DM = await LoadedUser.GetOrCreateDMChannelAsync();
						await DM.SendMessageAsync(embed: RemindEmbed);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "RaidNotification in DM of user");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "RaidNotification Global");
			}

		}

		public Embed MilestoneEmbed(Milestone milestone)
		{

			var embed = new EmbedBuilder
			{
				Title = $"{milestone.DateExpire.ToString("dd.MM.yyyy")}, в {milestone.DateExpire.ToString("HH:mm")} по {milestone.DateExpire.Kind}. {milestone.MilestoneInfo.Type}: {milestone.MilestoneInfo.Name}",
				ThumbnailUrl = milestone.MilestoneInfo.Icon,
				Color = Color.DarkMagenta

			};
			if (milestone.Note != null)
				embed.WithDescription($"**Заметка от лидера:** {milestone.Note}");

			var leader = _discord.GetUser(milestone.Leader);

			embed.AddField("Информация",
			$"- Лидер боевой группы: **#1 {leader.Mention} - {leader.Username}**\n" +
			$"- Чтобы за вами закрепилось место нажмите на реакцию {_emote.Raid}");

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"В боевую группу записались"
			};
			int count = 2;
			foreach (var user in milestone.MilestoneUsers)
			{
				var discordUser = _discord.GetUser(user.UserId);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			return embed.Build();
		}

		private Embed MilestoneRemindEmbed(Milestone milestone)
		{
			var guild = _discord.GetGuild(milestone.GuildId);

			var authorBuilder = new EmbedAuthorBuilder
			{
				Name = $"Доброго времени суток, страж.",
				IconUrl = _discord.CurrentUser.GetAvatarUrl()
			};

			var embed = new EmbedBuilder()
			{
				Title = $"Хочу вам напомнить, что у вас через 15 минут начнется **{milestone.MilestoneInfo.Type}**.",
				Author = authorBuilder,
				Color = Color.DarkMagenta,
				ThumbnailUrl = milestone.MilestoneInfo.Icon
			};
			if (milestone.Note != null)
				embed.WithDescription($"**Заметка:** {milestone.Note}");

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"Состав боевой группы"
			};
			var leader = _discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}";
			int count = 2;
			foreach (var user in milestone.MilestoneUsers)
			{

				var discordUser = _discord.GetUser(user.UserId);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";

				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			embed.WithFooter($"{milestone.MilestoneInfo.Type}: {milestone.MilestoneInfo.Name}. Сервер: {guild.Name}", guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}
	}
}
