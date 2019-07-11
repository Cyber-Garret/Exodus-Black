
using System;
using System.Collections.Generic;
using System.Text;

using Discord;
using Discord.WebSocket;

using Core;

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

		internal static EmbedBuilder StartRaidEmbed(SocketUser user, RaidName raid, DateTime date)
		{
			EmbedBuilder embed = new EmbedBuilder();

			var raidInfo = RaidsHelpers.GetRaidInfo(raid);

			embed.WithTitle($"{date.Date.ToShortDateString()} в {date.TimeOfDay} по МСК. Рейд: {raidInfo.Item1}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
			embed.WithDescription(raidInfo.Item2);

			embed.AddField("Рейд лидер", user.Mention);
			embed.AddField("Страж #2", "Свободно");
			embed.AddField("Страж #3", "Свободно");
			embed.AddField("Страж #4", "Свободно");
			embed.AddField("Страж #5", "Свободно");
			embed.AddField("Страж #6", "Свободно");
			embed.WithFooter("Что бы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		internal static void RegisterRaid(ulong msgId, string GuildName, ulong raidLeader, RaidName raid, DateTime date)
		{
			try
			{
				var raidInfo = RaidsHelpers.GetRaidInfo(raid);
				using (FailsafeContext Db = new FailsafeContext())
				{
					Core.Models.Db.ActiveRaid activeRaid = new Core.Models.Db.ActiveRaid
					{
						Id = msgId,
						Guild = GuildName,
						Name = raidInfo.Item1,
						Description = raidInfo.Item2,
						DateExpire = date,
						User1 = raidLeader,
						User2 = 0,
						User3 = 0,
						User4 = 0,
						User5 = 0,
						User6 = 0
					};
					Db.ActiveRaids.Add(activeRaid);
					Db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Logger.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
			}

		}
	}
}
