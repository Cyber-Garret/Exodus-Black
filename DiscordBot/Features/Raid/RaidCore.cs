using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Core.Models.Db;

namespace DiscordBot.Features.Raid
{
	internal static class RaidsCore
	{
		public static readonly Dictionary<string, IEmote> ReactOptions;


		static RaidsCore()
		{
			ReactOptions = new Dictionary<string, IEmote>
			{
				{ "2", new Emoji("2\u20e3")},
				{ "3", new Emoji("3\u20e3")},
				{ "4", new Emoji("4\u20e3")},
				{ "5", new Emoji("5\u20e3")},
				{ "6", new Emoji("6\u20e3")}
			};
		}

		internal static EmbedBuilder StartRaidEmbed(SocketUser user, RaidInfo info, DateTime date)
		{
			EmbedBuilder embed = new EmbedBuilder();

			embed.WithTitle($"{date.Date.ToShortDateString()} в {date.TimeOfDay} по МСК. Рейд: {info.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
			embed.WithDescription(info.PreviewDesc);

			embed.AddField("Рейд лидер", user.Mention);
			embed.AddField("Страж #2", "Свободно");
			embed.AddField("Страж #3", "Свободно");
			embed.AddField("Страж #4", "Свободно");
			embed.AddField("Страж #5", "Свободно");
			embed.AddField("Страж #6", "Свободно");
			embed.WithFooter("Что бы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		internal static async Task RegisterRaidAsync(ulong msgId, string GuildName, ulong raidLeader, int raidInfoId, DateTime date)
		{
			try
			{
				var newRaid = new ActiveRaid
				{
					MessageId = msgId,
					Guild = GuildName,
					RaidInfoId = raidInfoId,
					DateExpire = date,
					User1 = raidLeader,
					User2 = 0,
					User3 = 0,
					User4 = 0,
					User5 = 0,
					User6 = 0
				};
				await FailsafeDbOperations.SaveRaidAsync(newRaid);
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
			}

		}
	}
}
