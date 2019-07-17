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

		internal static EmbedBuilder StartRaidEmbed(SocketUser user, RaidInfo info, DateTime date, string userMemo)
		{
			EmbedBuilder embed = new EmbedBuilder();

			embed.WithTitle($"{date.Date.ToString("dd.MM.yyyy")}, {Global.culture.DateTimeFormat.GetDayName(date.DayOfWeek)} в {date.ToString("HH:mm")} по МСК. {info.Type}: {info.Name}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl(info.Icon);
			if (info.PreviewDesc != null)
				embed.WithDescription(info.PreviewDesc);
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

		internal static async Task RegisterRaidAsync(ulong msgId, string GuildName, ulong raidLeader, int raidInfoId, DateTime date, string userMemo)
		{
			try
			{
				var newRaid = new ActiveRaid
				{
					MessageId = msgId,
					Guild = GuildName,
					RaidInfoId = raidInfoId,
					Memo = userMemo,
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

		internal static async void HandleReaction(IUserMessage message, ActiveRaid activeRaid)
		{
			var newEmbed = await RaidsHelpers.RebuildEmbedAsync(activeRaid);
			if (newEmbed.Length != 0)
				await RaidsHelpers.UpdateMessage(message, newEmbed);

			await FailsafeDbOperations.SaveRaidAsync(activeRaid);
		}
	}
}
