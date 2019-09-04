using Bot.Models.Db.Destiny2;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using System;
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

		public EmbedBuilder StartEmbed(SocketUser user, Milestone milestone, DateTime date, string userMemo)
		{
			var embed = new EmbedBuilder();

			embed.WithTitle($"{date.Date.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(date.DayOfWeek)} в {date.ToString("HH:mm")} по МСК. {milestone.Type}: {milestone.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl(milestone.Icon);
			if (milestone.PreviewDesc != null)
				embed.WithDescription(milestone.PreviewDesc);
			if (userMemo != null)
				embed.AddField("Заметка от лидера", userMemo);
			embed.AddField("Лидер боевой группы", $"{user.Mention} - {user.Username}");
			embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		public EmbedBuilder RebuildEmbed(ActiveMilestone activeMilestone)
		{
			var embed = new EmbedBuilder()
				.WithTitle($"{activeMilestone.DateExpire.ToShortDateString()}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}")
				.WithColor(Color.DarkMagenta)
				.WithThumbnailUrl(activeMilestone.Milestone.Icon);
			if (activeMilestone.Milestone.PreviewDesc != null)
				embed.WithDescription(activeMilestone.Milestone.PreviewDesc);
			if(activeMilestone.Memo != null)
				embed.AddField("Заметка от лидера", activeMilestone.Memo);
			var milestoneLeader = Client.GetUser(activeMilestone.Leader);
			embed.AddField("Лидер боевой группы", $"{milestoneLeader.Mention} - {milestoneLeader.Username}");
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
