using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

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
						raid.User2 = reaction.User.Value.Id;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
				{
					if (raid.User3 == 0)
						raid.User3 = reaction.User.Value.Id;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
				{
					if (raid.User4 == 0)
						raid.User4 = reaction.User.Value.Id;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
				{
					if (raid.User5 == 0)
						raid.User5 = reaction.User.Value.Id;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
				{
					if (raid.User6 == 0)
						raid.User6 = reaction.User.Value.Id;
				}
				//await RaidsCore.HandleReaction(msg, reaction);
				var newEmbed = await RebuildEmbedAsync(raid);
				if (newEmbed.Length != 0)
					await UpdateMessage(msg, newEmbed);

				await FailsafeDbOperations.SaveRaidAsync(raid);
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
						raid.User2 = 0;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["3"]))
				{
					if (raid.User3 == reaction.User.Value.Id)
						raid.User3 = 0;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["4"]))
				{
					if (raid.User4 == reaction.User.Value.Id)
						raid.User4 = 0;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["5"]))
				{
					if (raid.User5 == reaction.User.Value.Id)
						raid.User5 = 0;
				}
				if (reaction.Emote.Equals(RaidsCore.ReactOptions["6"]))
				{
					if (raid.User6 == reaction.User.Value.Id)
						raid.User6 = 0;
				}
				//await RaidsCore.HandleReaction(msg, reaction);
				var newEmbed = await RebuildEmbedAsync(raid);
				if (newEmbed.Length != 0)
					await UpdateMessage(msg, newEmbed);

				await FailsafeDbOperations.SaveRaidAsync(raid);

			}
		}

		internal static async Task<EmbedBuilder> RebuildEmbedAsync(Core.Models.Db.ActiveRaid raid)
		{
			try
			{
				EmbedBuilder embed = new EmbedBuilder();

				embed.WithTitle($"{raid.DateExpire.ToString("d.M.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(raid.DateExpire.DayOfWeek)} в {raid.DateExpire.ToString("HH:mm")} по МСК. Рейд: {raid.RaidInfo.Name}");
				embed.WithColor(Color.DarkMagenta);
				embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
				embed.WithDescription($"**Заметка от рейд-лидера:**\n" + raid.Memo);

				embed.AddField("Рейд лидер", Program.Client.GetUser(raid.User1).Mention);

				if (raid.User2 != 0)
					embed.AddField("Страж #2", Program.Client.GetUser(raid.User2).Mention);
				else
					embed.AddField("Страж #2", "Свободно");
				if (raid.User3 != 0)
					embed.AddField("Страж #3", Program.Client.GetUser(raid.User3).Mention);
				else
					embed.AddField("Страж #3", "Свободно");
				if (raid.User4 != 0)
					embed.AddField("Страж #4", Program.Client.GetUser(raid.User4).Mention);
				else
					embed.AddField("Страж #4", "Свободно");
				if (raid.User5 != 0)
					embed.AddField("Страж #5", Program.Client.GetUser(raid.User5).Mention);
				else
					embed.AddField("Страж #5", "Свободно");
				if (raid.User6 != 0)
					embed.AddField("Страж #6", Program.Client.GetUser(raid.User6).Mention);
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

		private static async Task UpdateMessage(IUserMessage socketMsg, EmbedBuilder embed)
		{
			await socketMsg.ModifyAsync(message =>
			{
				message.Embed = embed.Build();
			});
		}
	}
}
