using Bot.Models.Db;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class MilestoneService
	{
		private readonly DiscordSocketClient Client;
		private readonly FailsafeContext Db;
		private SocketGuild NeiraHome;
		private IEmote RaidEmote;
		private IEmote RaidNewbie;
		public MilestoneService(DiscordSocketClient socketClient, FailsafeContext context)
		{
			Client = socketClient;
			Db = context;
		}

		public void Initialize()
		{
			NeiraHome = Client.GetGuild(521689023962415104);
			RaidEmote = NeiraHome.Emotes.First(e => e.Name == "Neira_Raid");
			RaidNewbie = NeiraHome.Emotes.First(e => e.Name == "Neira_New");
		}
		public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(RaidEmote))
				{
					//get milestone
					var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

					if (milestone == null) return;

					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

					if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count < 6)
					{
						Db.MilestoneUsers.Add(new MilestoneUser
						{
							MessageId = milestone.MessageId,
							UserId = reaction.UserId
						});
						await Db.SaveChangesAsync();
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = Client.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(RaidEmote, user);
					}
				}

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "Reaction Added in Milestone", ex.Message, ex));
			}
		}

		internal async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(RaidEmote))
				{
					//get milestone
					var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Include(mu => mu.MilestoneUsers).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

					if (milestone == null) return;

					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

					if (reaction.UserId != milestone.Leader && UserExist && milestone.MilestoneUsers.Count < 6)
					{
						var milestoneUser = Db.MilestoneUsers.First(u => u.UserId == reaction.UserId && u.MessageId == milestone.MessageId);

						Db.Remove(milestoneUser);
						await Db.SaveChangesAsync();
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = Client.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(RaidEmote, user);
					}
				}

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "Reaction Added in Milestone", ex.Message, ex));
			}
		}

		internal async void HandleReaction(IUserMessage message, ActiveMilestone activeMilestone)
		{
			var newEmbed = RebuildEmbed(activeMilestone);
			if (newEmbed.Length != 0)
				await message.ModifyAsync(m => m.Embed = newEmbed.Build());
		}

		public EmbedBuilder StartEmbed(SocketUser user, Milestone milestone, DateTime date, string userMemo)
		{
			var embed = new EmbedBuilder();

			embed.WithTitle($"{date.Date.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(date.DayOfWeek)} в {date.ToString("HH:mm")} по МСК. {milestone.Type}: {milestone.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl(milestone.Icon);
			if (milestone.PreviewDesc != null)
				embed.WithDescription(milestone.PreviewDesc);

			var embedfield = new EmbedFieldBuilder
			{
				Name = "Информация",
				Value =
				$"- Лидер боевой группы: **{user.Mention} - {user.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {RaidEmote}\n" +
				$"- Если вы новичок в данной активности нажмите на реакцию {RaidNewbie}"
			};

			if (userMemo != null)
				embedfield.Value += $"- Заметка: **{userMemo}**";

			embed.AddField(embedfield);

			return embed;
		}

		public EmbedBuilder RebuildEmbed(ActiveMilestone activeMilestone)
		{
			var embed = new EmbedBuilder()
				.WithTitle($"{activeMilestone.DateExpire.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}")
				.WithColor(Color.DarkMagenta)
				.WithThumbnailUrl(activeMilestone.Milestone.Icon);
			if (activeMilestone.Milestone.PreviewDesc != null)
				embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

			var milestoneLeader = Client.GetUser(activeMilestone.Leader);
			var embedfieldInfo = new EmbedFieldBuilder
			{
				Name = "Информация",
				Value =
				$"- Лидер боевой группы: **{milestoneLeader.Mention} - {milestoneLeader.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {RaidEmote}\n" +
				$"- Если вы новичок в данной активности нажмите на реакцию {RaidNewbie}"
			};

			if (activeMilestone.Memo != null)
				embedfieldInfo.Value += $"- Заметка: **{activeMilestone.Memo}**";

			embed.AddField(embedfieldInfo);

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"В боевую группу записались"
			};

			foreach (var user in activeMilestone.MilestoneUsers)
			{
				var discordUser = Client.GetUser(user.UserId);
				embedFieldUsers.Value += $"{discordUser.Mention} - {discordUser.Username}\n";

			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			return embed;
		}

		internal async Task RegisterMilestoneAsync(ulong msgId, SocketCommandContext context, int raidInfoId, DateTime date, string userMemo)
		{
			try
			{
				ActiveMilestone newMilestone = new ActiveMilestone
				{
					MessageId = msgId,
					TextChannelId = context.Channel.Id,
					GuildId = context.Guild.Id,
					MilestoneId = raidInfoId,
					Memo = userMemo,
					DateExpire = date,
					Leader = context.User.Id
				};

				Db.ActiveMilestones.Add(newMilestone);
				await Db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Register Milestone Method", ex.Message, ex));
			}

		}
	}
}
