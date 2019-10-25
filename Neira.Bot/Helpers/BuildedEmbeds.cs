using Discord;
using Neira.Bot.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neira.Bot.Helpers
{
	internal class BuildedEmbeds
	{
		internal static EmbedBuilder ClanStatus(Clan clan)
		{

			var embed = new EmbedBuilder();
			embed.WithTitle($"Онлайн статус стражей клана `{clan.Name}`");
			embed.WithColor(Color.Gold);
			////Bungie Clan link
			embed.WithUrl($"https://www.bungie.net/ru/ClanV2?groupid={clan.Id}");
			////Some clan main info
			embed.WithDescription(
				$"В данный момент в клане **{clan.MemberCount}**/100 стражей.");

			#region list for member sorted for some days
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
				embed.AddField("Был(a) сегодня", ThisDay);
			//Same as above, but who enter to the game yesterday
			string Yesterday = string.Join(", ", _Yesterday);
			if (!string.IsNullOrEmpty(Yesterday))
				embed.AddField("Был(a) вчера", Yesterday);
			//Same as above, but who enter to the game more 5 days but fewer 7 days ago
			string ThisWeek = string.Join(", ", _ThisWeek);
			if (!string.IsNullOrEmpty(ThisWeek))
				embed.AddField("Был(a) в течение 7 дней", ThisWeek);
			//Same as above, but who enter to the game more 7 days ago
			string MoreOneWeek = string.Join(", ", _MoreOneWeek);
			if (!string.IsNullOrEmpty(MoreOneWeek))
				embed.AddField("Был(a) больше 7 дней тому назад", MoreOneWeek);
			//For user who not have any data.
			string NoData = string.Join(", ", _NoData);
			if (!string.IsNullOrEmpty(NoData))
				embed.AddField("Нет данных", NoData);
			//Simple footer with clan name
			embed.WithFooter($"Данные об онлайн стражей, клане и его составе обновляются каждые 15 минут.");

			return embed;
		}

		internal static Embed BaseGlimmerEmbed(Color color, string text, string title = null, EmbedFooterBuilder footerBuilder = null)
		{
			var embed = new EmbedBuilder
			{
				Color = color,
				Description = text
			};
			if (!string.IsNullOrWhiteSpace(title))
				embed.Title = title;

			if (footerBuilder != null)
				embed.Footer = footerBuilder;

			return embed.Build();
		}
	}
}
