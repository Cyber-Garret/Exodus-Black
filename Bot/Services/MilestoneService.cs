﻿using Bot.Models.Db.Destiny2;

using Discord;
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

		internal async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			//get milestone
			var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();
			
			if (milestone != null)
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(Global.ReactPlaceNumber["2"]))
				{
					if (milestone.User2 == 0)
					{
						milestone.User2 = reaction.User.Value.Id;
						HandleReaction(msg, milestone);
					}

				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["3"]))
				{
					if (milestone.User3 == 0)
					{
						milestone.User3 = reaction.User.Value.Id;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["4"]))
				{
					if (milestone.User4 == 0)
					{
						milestone.User4 = reaction.User.Value.Id;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["5"]))
				{
					if (milestone.User5 == 0)
					{
						milestone.User5 = reaction.User.Value.Id;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["6"]))
				{
					if (milestone.User6 == 0)
					{
						milestone.User6 = reaction.User.Value.Id;
						HandleReaction(msg, milestone);
					}
				}

			}

		}

		internal async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			var milestone = await Db.ActiveMilestones.Include(r => r.Milestone).Where(r => r.MessageId == cache.Id).FirstOrDefaultAsync();

			if (milestone != null)
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(Global.ReactPlaceNumber["2"]))
				{
					if (milestone.User2 == reaction.User.Value.Id)
					{
						milestone.User2 = 0;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["3"]))
				{
					if (milestone.User3 == reaction.User.Value.Id)
					{
						milestone.User3 = 0;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["4"]))
				{
					if (milestone.User4 == reaction.User.Value.Id)
					{
						milestone.User4 = 0;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["5"]))
				{
					if (milestone.User5 == reaction.User.Value.Id)
					{
						milestone.User5 = 0;
						HandleReaction(msg, milestone);
					}
				}
				else if (reaction.Emote.Equals(Global.ReactPlaceNumber["6"]))
				{
					if (milestone.User6 == reaction.User.Value.Id)
					{
						milestone.User6 = 0;
						HandleReaction(msg, milestone);
					}
				}
			}
		}

		internal async Task<EmbedBuilder> RebuildEmbedAsync(ActiveMilestone activeMilestone)
		{
			try
			{
				EmbedBuilder embed = new EmbedBuilder();

				embed.WithTitle($"{activeMilestone.DateExpire.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}");
				embed.WithColor(Color.DarkMagenta);
				embed.WithThumbnailUrl(activeMilestone.Milestone.Icon);
				if (activeMilestone.Milestone.PreviewDesc != null)
					embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

				if (activeMilestone.Memo != null)
					embed.AddField("Заметка от лидера", activeMilestone.Memo);

				embed.AddField("Страж #1", $"{Client.GetUser(activeMilestone.User1).Mention} - {Client.GetUser(activeMilestone.User1).Username}");

				if (activeMilestone.User2 != 0)
				{
					var user = Client.GetUser(activeMilestone.User2);
					embed.AddField("Страж #2", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #2", "Свободно");

				if (activeMilestone.User3 != 0)
				{
					var user = Client.GetUser(activeMilestone.User3);
					embed.AddField("Страж #3", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #3", "Свободно");

				if (activeMilestone.User4 != 0)
				{
					var user = Client.GetUser(activeMilestone.User4);
					embed.AddField("Страж #4", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #4", "Свободно");

				if (activeMilestone.User5 != 0)
				{
					var user = Client.GetUser(activeMilestone.User5);
					embed.AddField("Страж #5", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #5", "Свободно");

				if (activeMilestone.User6 != 0)
				{
					var user = Client.GetUser(activeMilestone.User6);
					embed.AddField("Страж #6", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #6", "Свободно");

				embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

				return embed;
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "RebuildEmbedAsync", ex.Message, ex));
				return new EmbedBuilder();
			}

		}


		public EmbedBuilder StartEmbed(SocketUser user, Milestone milestone, DateTime date, string userMemo)
		{
			EmbedBuilder embed = new EmbedBuilder();

			embed.WithTitle($"{date.Date.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(date.DayOfWeek)} в {date.ToString("HH:mm")} по МСК. {milestone.Type}: {milestone.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl(milestone.Icon);
			if (milestone.PreviewDesc != null)
				embed.WithDescription(milestone.PreviewDesc);
			if (userMemo != null)
				embed.AddField("Заметка от лидера", userMemo);
			embed.AddField("Страж #1", $"{user.Mention} - {user.Username}");
			embed.AddField("Страж #2", "Свободно");
			embed.AddField("Страж #3", "Свободно");
			embed.AddField("Страж #4", "Свободно");
			embed.AddField("Страж #5", "Свободно");
			embed.AddField("Страж #6", "Свободно");
			embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		internal async Task RegisterMilestoneAsync(ulong msgId, string GuildName, ulong raidLeader, int raidInfoId, DateTime date, string userMemo)
		{
			try
			{
				ActiveMilestone newMilestone = new ActiveMilestone
				{
					MessageId = msgId,
					GuildName = GuildName,
					MilestoneId = raidInfoId,
					Memo = userMemo,
					DateExpire = date,
					User1 = raidLeader,
					User2 = 0,
					User3 = 0,
					User4 = 0,
					User5 = 0,
					User6 = 0
				};
				Db.ActiveMilestones.Add(newMilestone);
				await Db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "RegisterRaidAsync", ex.Message, ex));
			}

		}

		internal async void HandleReaction(IUserMessage message, ActiveMilestone activeMilestone)
		{
			var newEmbed = await RebuildEmbedAsync(activeMilestone);
			if (newEmbed.Length != 0)
			{
				await message.ModifyAsync(m => m.Embed = newEmbed.Build());
				Db.ActiveMilestones.Update(activeMilestone);
				await Db.SaveChangesAsync();
			}
		}
	}
}