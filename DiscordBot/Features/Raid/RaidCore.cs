
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

		internal static EmbedBuilder RaidEmbed(SocketUser user, string raidname, string date)
		{
			EmbedBuilder embed = new EmbedBuilder();

			var raidEnum = RaidEnumFromString(raidname);

			if (raidEnum == RaidName.None)
				return embed;

			var _raidTime = DateTime.Parse(date);
			var raidInfo = GetRaidInfo(raidEnum);

			embed.WithTitle($"{_raidTime.Date.ToShortDateString()} в {_raidTime.TimeOfDay} по МСК. Рейд: {raidInfo.Item1}");
			embed.WithColor(Color.DarkMagenta);
			embed.WithThumbnailUrl("http://neira.link/img/Raid_emblem.png");
			embed.WithDescription($"**О рейде:** {raidInfo.Item2}");

			embed.AddField("Рейд лидер", user.Mention);
			embed.AddField("Страж #2", "Свободно");
			embed.AddField("Страж #3", "Свободно");
			embed.AddField("Страж #4", "Свободно");
			embed.AddField("Страж #5", "Свободно");
			embed.AddField("Страж #6", "Свободно");
			embed.WithFooter("Что бы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed;
		}

		internal static void RegisterRaid(ulong msgId, ulong firstUser, string raidname, string date)
		{
			try
			{
				var raidEnum = RaidEnumFromString(raidname);

				if (raidEnum == RaidName.None)
					return;
				using (FailsafeContext Db = new FailsafeContext())
				{
					var _raidTime = DateTime.Parse(date);
					var raidInfo = GetRaidInfo(raidEnum);

					Core.Models.Db.ActiveRaid raid = new Core.Models.Db.ActiveRaid
					{
						Id = msgId,
						Name = raidInfo.Item1,
						Description = raidInfo.Item2,
						DateExpire = _raidTime,
						User1 = firstUser,
						User2 = 0,
						User3 = 0,
						User4 = 0,
						User5 = 0,
						User6 = 0
					};
					Db.ActiveRaids.Add(raid);
					Db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Logger.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
			}

		}

		#region Functions
		static (string, string) GetRaidInfo(RaidName raid)
		{

			switch (raid)
			{
				case RaidName.Leviathan:
					return ("Левиафан", "Короткое инфо о левике");
				case RaidName.EaterOfWorlds:
					return ("Пожиратель миров", "Короткое инфо о пожирателе");
				case RaidName.SpireOfStars:
					return ("Звездный шпиль", "Короткое инфо о шпиле");
				case RaidName.LastWish:
					return ("Последнее желание", "Короткое инфо о пж");
				case RaidName.ScourgeOfThePast:
					return ("Истребители прошлого", "Короткое инфо о ип");
				case RaidName.CrownOfSorrow:
					return ("Корона скорби", "Короткое инфо о короне");
				default:
					return ("Неизвестно", "Неизвестный мне рейд.");
			}
		}
		static RaidName RaidEnumFromString(string raidname)
		{
			if (raidname == "Левик")
				return RaidName.Leviathan;
			else if (raidname == "пм")
				return RaidName.EaterOfWorlds;
			else if (raidname == "зш")
				return RaidName.SpireOfStars;
			else if (raidname == "пж")
				return RaidName.LastWish;
			else if (raidname == "ип")
				return RaidName.ScourgeOfThePast;
			else if (raidname == "кс")
				return RaidName.CrownOfSorrow;
			else
				return RaidName.None;
		}
		#endregion
	}
}
