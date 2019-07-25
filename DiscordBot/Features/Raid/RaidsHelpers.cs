using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Core.Models.Destiny2;

namespace DiscordBot.Features.Raid
{
	internal static class RaidsHelpers
	{
		internal static async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			//get raid
			var raid = await FailsafeDbOperations.GetActiveMilestone(cache.Id);

			if (raid != null)
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
				{
					if (raid.User2 == 0)
					{
						raid.User2 = reaction.User.Value.Id;
						RaidsCore.HandleReaction(msg, raid);
					}

				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
				{
					if (raid.User3 == 0)
					{
						raid.User3 = reaction.User.Value.Id;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
				{
					if (raid.User4 == 0)
					{
						raid.User4 = reaction.User.Value.Id;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
				{
					if (raid.User5 == 0)
					{
						raid.User5 = reaction.User.Value.Id;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
				{
					if (raid.User6 == 0)
					{
						raid.User6 = reaction.User.Value.Id;
						RaidsCore.HandleReaction(msg, raid);
					}
				}

			}

		}

		internal static async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			var raid = await FailsafeDbOperations.GetActiveMilestone(cache.Id);

			if (raid != null)
			{
				var msg = await cache.GetOrDownloadAsync();

				if (reaction.Emote.Equals(RaidsCore.ReactOptions["2"]))
				{
					if (raid.User2 == reaction.User.Value.Id)
					{
						raid.User2 = 0;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
				{
					if (raid.User3 == reaction.User.Value.Id)
					{
						raid.User3 = 0;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
				{
					if (raid.User4 == reaction.User.Value.Id)
					{
						raid.User4 = 0;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
				{
					if (raid.User5 == reaction.User.Value.Id)
					{
						raid.User5 = 0;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
				else if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
				{
					if (raid.User6 == reaction.User.Value.Id)
					{
						raid.User6 = 0;
						RaidsCore.HandleReaction(msg, raid);
					}
				}
			}
		}

		internal static async Task<EmbedBuilder> RebuildEmbedAsync(ActiveMilestone activeMilestone)
		{
			try
			{
				EmbedBuilder embed = new EmbedBuilder();

				embed.WithTitle($"{activeMilestone.DateExpire.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(activeMilestone.DateExpire.DayOfWeek)} в {activeMilestone.DateExpire.ToString("HH:mm")} по МСК. {activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}");
				embed.WithColor(Color.DarkMagenta);
				embed.WithThumbnailUrl(activeMilestone.Milestone.Icon);
				if (activeMilestone.Milestone.PreviewDesc != null)
					embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

				embed.AddField("Заметка от лидера", activeMilestone.Memo);

				embed.AddField("Страж #1", $"{Program.Client.GetUser(activeMilestone.User1).Mention} - {Program.Client.GetUser(activeMilestone.User1).Username}");

				if (activeMilestone.User2 != 0)
				{
					var user = Program.Client.GetUser(activeMilestone.User2);
					embed.AddField("Страж #2", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #2", "Свободно");

				if (activeMilestone.User3 != 0)
				{
					var user = Program.Client.GetUser(activeMilestone.User3);
					embed.AddField("Страж #3", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #3", "Свободно");

				if (activeMilestone.User4 != 0)
				{
					var user = Program.Client.GetUser(activeMilestone.User4);
					embed.AddField("Страж #4", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #4", "Свободно");

				if (activeMilestone.User5 != 0)
				{
					var user = Program.Client.GetUser(activeMilestone.User5);
					embed.AddField("Страж #5", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #5", "Свободно");

				if (activeMilestone.User6 != 0)
				{
					var user = Program.Client.GetUser(activeMilestone.User6);
					embed.AddField("Страж #6", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #6", "Свободно");

				embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

				return embed;
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				return new EmbedBuilder();
			}

		}

		internal static async Task UpdateMessage(IUserMessage socketMsg, EmbedBuilder embed)
		{
			await socketMsg.ModifyAsync(message =>
			{
				message.Embed = embed.Build();
			});
		}
	}
}
