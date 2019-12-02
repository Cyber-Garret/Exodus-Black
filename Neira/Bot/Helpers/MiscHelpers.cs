using Discord;
using Discord.WebSocket;
using Neira.Database;
using System;
using System.Collections.Generic;

namespace Neira.Bot.Helpers
{
	class MiscHelpers
	{
		public static EmbedBuilder WelcomeEmbed(SocketGuildUser guildUser, string text)
		{

			var embed = new EmbedBuilder()
			{
				Color = Color.Orange,
				Title = $"Добро пожаловать на сервер {guildUser.Guild.Name}",
				Description = text
			};
			//if guild have picture add to message.
			if (!string.IsNullOrEmpty(guildUser.Guild.IconUrl))
				embed.ThumbnailUrl = guildUser.Guild.IconUrl;

			return embed;
		}

		internal static string ClanStatus(Clan clan)
		{
			string message = $"В данный момент в [`{clan.Name}`](http://neira.su/Clan/{clan.Id}) **{clan.MemberCount}**/100 стражей.\n";

			#region lists for guardians sorted by specific last online date 
			List<string> _ThisDay = new List<string>();
			List<string> _Yesterday = new List<string>();
			List<string> _ThisWeek = new List<string>();
			List<string> _MoreOneWeek = new List<string>();
			List<string> _NoData = new List<string>();
			#endregion

			//Main Sorting logic
			foreach (var member in clan.Members)
			{
				int LastOnlineTime = 1000;
				//Property for calculate how long days user did not enter the Destiny
				if (member.DateLastPlayed != null)
					LastOnlineTime = (DateTime.Today.Date - member.DateLastPlayed.Value.Date).Days;

				//Sorting user to right list
				if (LastOnlineTime < 1)
				{
					_ThisDay.Add(member.Name);
				}
				else if (LastOnlineTime >= 1 && LastOnlineTime < 2)
				{
					_Yesterday.Add(member.Name);
				}
				else if (LastOnlineTime >= 2 && LastOnlineTime <= 7)
				{
					_ThisWeek.Add(member.Name);
				}
				else if (LastOnlineTime >= 7 && LastOnlineTime < 500)
				{
					_MoreOneWeek.Add(member.Name);
				}
				else if (LastOnlineTime > 500)
				{
					_NoData.Add(member.Name);
				}
			}

			//Create one string who enter to the game today, like "Petya,Vasia,Grisha",
			//and if string ThisDay not empty add to embed message special field.
			string ThisDay = string.Join(", ", _ThisDay);
			if (!string.IsNullOrEmpty(ThisDay))
				message += "**Был(a) сегодня**\n" + ThisDay;
			//Same as above, but who enter to the game yesterday
			string Yesterday = string.Join(", ", _Yesterday);
			if (!string.IsNullOrEmpty(Yesterday))
				message += "\n**Был(a) вчера**\n" + Yesterday;
			//Same as above, but who enter to the game more 5 days but fewer 7 days ago
			string ThisWeek = string.Join(", ", _ThisWeek);
			if (!string.IsNullOrEmpty(ThisWeek))
				message += "\n**Был(a) в течение 7 дней**\n" + ThisWeek;
			//Same as above, but who enter to the game more 7 days ago
			string MoreOneWeek = string.Join(", ", _MoreOneWeek);
			if (!string.IsNullOrEmpty(MoreOneWeek))
				message += "\n**Был(a) больше 7 дней тому назад**\n" + MoreOneWeek;
			//For user who not have any data.
			string NoData = string.Join(", ", _NoData);
			if (!string.IsNullOrEmpty(NoData))
				message += "\n**Нет данных**\n" + NoData;

			return message;
		}
	}
}
