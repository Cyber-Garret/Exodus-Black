using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Neira.Db;
using Neira.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neira.Bot.Services
{
	public class MilestoneService
	{
		private readonly DiscordSocketClient Client;
		private readonly NeiraContext Db;
		private SocketGuild NeiraHome;
		public IEmote RaidEmote;
		public MilestoneService(DiscordSocketClient socketClient, NeiraContext context)
		{
			Client = socketClient;
			Db = context;
		}

		public void Initialize()
		{
			NeiraHome = Client.GetGuild(521689023962415104);
			RaidEmote = NeiraHome.Emotes.First(e => e.Name == "Neira_Raid");
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

					if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count < 5)
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

		public async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
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

					if (reaction.UserId != milestone.Leader && UserExist)
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

		private async void HandleReaction(IUserMessage message, ActiveMilestone activeMilestone)
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

			embed.AddField("Информация",
				$"- Лидер боевой группы: **#1 {user.Mention} - {user.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {RaidEmote}");

			if (userMemo != null)
				embed.AddField("Заметка от лидера:", userMemo);

			return embed;
		}

		public async Task RegisterMilestoneAsync(ulong msgId, SocketCommandContext context, int raidInfoId, DateTime date, string userMemo)
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

				_ = Task.Run(() =>
				{
					var stats = Db.BotInfos.FirstOrDefault();
					stats.Milestones++;
					stats.Servers = Client.Guilds.Count;
					stats.Users = Client.Guilds.Sum(u => u.Users.Count);
					Db.BotInfos.Update(stats);
					Db.SaveChanges();
				});
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "Register Milestone Method", ex.Message, ex));
			}

		}

		private EmbedBuilder RebuildEmbed(ActiveMilestone activeMilestone)
		{
			var embed = new EmbedBuilder()
				.WithTitle($"{activeMilestone.DateExpire.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}")
				.WithColor(Color.DarkMagenta)
				.WithThumbnailUrl(activeMilestone.Milestone.Icon);
			if (activeMilestone.Milestone.PreviewDesc != null)
				embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

			var milestoneLeader = Client.GetUser(activeMilestone.Leader);
			embed.AddField("Информация",
				$"- Лидер боевой группы: **#1 {milestoneLeader.Mention} - {milestoneLeader.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {RaidEmote}\n");

			if (activeMilestone.Memo != null)
				embed.AddField("Заметка от лидера:", activeMilestone.Memo);

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"В боевую группу записались"
			};
			int count = 2;
			foreach (var user in activeMilestone.MilestoneUsers)
			{
				var discordUser = Client.GetUser(user.UserId);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			return embed;
		}

		public async Task RaidNotificationAsync(List<ulong> userIds, ActiveMilestone milestone)
		{
			foreach (var item in userIds)
			{
				if (item != 0)
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
						Thread.Sleep(1000);
					}
					catch (Exception ex)
					{
						await Logger.Log(new LogMessage(LogSeverity.Error, "RaidNotification", ex.Message, ex));
					}

				}
			}


		}
	}
}
