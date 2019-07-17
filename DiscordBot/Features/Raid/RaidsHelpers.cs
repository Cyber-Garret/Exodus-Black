using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Core.Models.Db;

namespace DiscordBot.Features.Raid
{
	internal static class RaidsHelpers
	{
		internal static async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			//get raid
			var raid = await FailsafeDbOperations.GetRaidAsync(cache.Id);

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
			var raid = await FailsafeDbOperations.GetRaidAsync(cache.Id);

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

		internal static async Task<EmbedBuilder> RebuildEmbedAsync(ActiveRaid raid)
		{
			try
			{
				EmbedBuilder embed = new EmbedBuilder();

				embed.WithTitle($"{raid.DateExpire.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(raid.DateExpire.DayOfWeek)} в {raid.DateExpire.ToString("HH:mm")} по МСК. {raid.RaidInfo.Type}: {raid.RaidInfo.Name}");
				embed.WithColor(Color.DarkMagenta);
				embed.WithThumbnailUrl(raid.RaidInfo.Icon);
				if (raid.RaidInfo.PreviewDesc != null)
					embed.WithDescription(raid.RaidInfo.PreviewDesc);

				embed.AddField("Заметка от лидера", raid.Memo);

				embed.AddField("Страж #1", $"{Program.Client.GetUser(raid.User1).Mention} - {Program.Client.GetUser(raid.User1).Username}");

				if (raid.User2 != 0)
				{
					var user = Program.Client.GetUser(raid.User2);
					embed.AddField("Страж #2", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #2", "Свободно");

				if (raid.User3 != 0)
				{
					var user = Program.Client.GetUser(raid.User3);
					embed.AddField("Страж #3", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #3", "Свободно");

				if (raid.User4 != 0)
				{
					var user = Program.Client.GetUser(raid.User4);
					embed.AddField("Страж #4", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #4", "Свободно");

				if (raid.User5 != 0)
				{
					var user = Program.Client.GetUser(raid.User5);
					embed.AddField("Страж #5", $"{user.Mention} - {user.Username}");
				}
				else
					embed.AddField("Страж #5", "Свободно");

				if (raid.User6 != 0)
				{
					var user = Program.Client.GetUser(raid.User6);
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
