using Bot.Models.Db.Destiny2;

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
		readonly DiscordSocketClient Client;
		readonly FailsafeContext Db;
		public MilestoneService(DiscordSocketClient socketClient, FailsafeContext context)
		{
			Client = socketClient;
			Db = context;
		}

		public EmbedBuilder StartEmbed(SocketUser user, Milestone milestone, int places, DateTime date, string userMemo)
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
				$"- Доступных мест: **{places}**\n"
			};

			if (userMemo != null)
				embedfield.Value += $"- Заметка: **{userMemo}**";

			embed.AddField(embedfield);
			embed.WithFooter("Чтобы за вами закрепилось место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		public EmbedBuilder RebuildEmbed(ActiveMilestone activeMilestone, List<ulong> users)
		{
			var embed = new EmbedBuilder()
				.WithTitle($"{activeMilestone.DateExpire.ToShortDateString()}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}")
				.WithColor(Color.DarkMagenta)
				.WithThumbnailUrl(activeMilestone.Milestone.Icon);
			if (activeMilestone.Milestone.PreviewDesc != null)
				embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

			var milestoneLeader = Client.GetUser(activeMilestone.Leader);
			var embedfield = new EmbedFieldBuilder
			{
				Name = "Информация",
				Value = $"- Лидер боевой группы: **{milestoneLeader.Mention} - {milestoneLeader.Username}**\n"
			};
			if (activeMilestone.Places - users.Count > 0)
				embedfield.Value += $"- Осталось мест: **{activeMilestone.Places - users.Count}**\n";

			if (activeMilestone.Memo != null)
				embedfield.Value += $"- Заметка: **{activeMilestone.Memo}**";

			embed.AddField(embedfield);

			var embedField = new EmbedFieldBuilder
			{
				Name = $"В боевую группу записались"
			};

			foreach (var user in users)
			{
				var discordUser = Client.GetUser(user);
				embedField.Value += $"{discordUser.Mention} - {discordUser.Username}\n";

			}
			embed.AddField(embedField);

			return embed;
		}

		internal async Task RegisterMilestoneAsync(ulong msgId, SocketCommandContext context, int numPlaces, int raidInfoId, DateTime date, string userMemo)
		{
			try
			{
				ActiveMilestone newMilestone = new ActiveMilestone
				{
					MessageId = msgId,
					TextChannelId = context.Channel.Id,
					GuildId = context.Guild.Id,
					Places = numPlaces,
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
