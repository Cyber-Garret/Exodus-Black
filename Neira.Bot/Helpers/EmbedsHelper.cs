﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Neira.Bot.Database;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Neira.Bot.Helpers
{
	public static class EmbedsHelper
	{
		#region Xur
		public static Embed XurArrived()
		{
			var embed = new EmbedBuilder
			{
				Title = "Стражи! Зур прибыл в солнечную систему!",
				Color = Color.Gold,
				ThumbnailUrl = "https://i.imgur.com/sFZZlwF.png",
				Description =
				"Мои алгоритмы глобального позиционирования пока еще в разработке потому определить точное местоположение Зур-а я не могу.\n" +
				"[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
				"[Или тут](https://ftw.in/game/destiny-2/find-xur)"
			};
			embed.WithFooter("Напоминаю! Зур покинет солнечную систему во вторник в 20:00 по МСК.");

			return embed.Build();
		}

		public static Embed XurLeave()
		{
			var embed = new EmbedBuilder
			{
				Title = "Внимание! Зур покинул солнечную систему.",
				Color = Color.Red,
				ThumbnailUrl = "https://i.imgur.com/sFZZlwF.png",
				Description = "Он просто испарился! Как только он придёт я сообщу."
			};
			embed.WithFooter("Напоминаю! В следующий раз Зур прибудет в пятницу в 20:00 по МСК.");

			return embed.Build();
		}
		#endregion

		#region Destiny 2
		public static EmbedBuilder ClanStatus(Clan clan)
		{

			var embed = new EmbedBuilder();
			embed.WithTitle($"Онлайн статус стражей клана `{clan.Name}`");
			embed.WithColor(Color.Gold);
			////Bungie Clan link
			embed.WithUrl($"http://neira.su/Clan/{clan.Id}");
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
		#endregion

		#region Milestone
		public static Embed MilestoneNew(SocketUser leader, Milestone milestone, IEmote raidEmote, string userMemo)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{milestone.Type}: {milestone.Name}",
				Color = Color.DarkMagenta,
				ThumbnailUrl = milestone.Icon
			};
			if (milestone.PreviewDesc != null)
				embed.WithDescription(milestone.PreviewDesc);

			embed.AddField("Информация",
				$"- Лидер боевой группы: **#1 {leader.Mention} - {leader.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {raidEmote}");

			if (!string.IsNullOrWhiteSpace(userMemo))
				embed.AddField("Заметка от лидера:", userMemo);

			return embed.Build();
		}

		public static Embed MilestoneNewV2(SocketUser leader, Milestone milestone, string userMemo)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{milestone.Type}: {milestone.Name}",
				Color = Color.Gold,
				ThumbnailUrl = milestone.Icon
			};

			if (milestone.PreviewDesc != null)
				embed.WithDescription(milestone.PreviewDesc);

			if (userMemo != null)
				embed.AddField("Заметка от лидера", userMemo);

			embed.AddField("Страж #1", $"{leader.Mention} - {leader.Username}");
			embed.AddField("Страж #2", "Свободно");
			embed.AddField("Страж #3", "Свободно");
			embed.AddField("Страж #4", "Свободно");
			embed.AddField("Страж #5", "Свободно");
			embed.AddField("Страж #6", "Свободно");
			embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed.Build();
		}

		public static Embed MilestoneRebuild(DiscordSocketClient Client, ActiveMilestone activeMilestone, IEmote raidEmote)
		{

			var embed = new EmbedBuilder
			{
				Title = $"{activeMilestone.Milestone.Type}: {activeMilestone.Milestone.Name}",
				Color = Color.DarkMagenta,
				ThumbnailUrl = activeMilestone.Milestone.Icon

			};
			if (activeMilestone.Milestone.PreviewDesc != null)
				embed.WithDescription(activeMilestone.Milestone.PreviewDesc);

			var leader = Client.GetUser(activeMilestone.Leader);

			embed.AddField("Информация",
				$"- Лидер боевой группы: **#1 {leader.Mention} - {leader.Username}**\n" +
				$"- Чтобы за вами закрепилось место нажмите на реакцию {raidEmote}");

			if (activeMilestone.Memo != null)
				embed.AddField("Заметка от лидера:", activeMilestone.Memo);

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = $"В боевую группу записались"
			};
			int count = 2;
			foreach (var user in activeMilestone.MilestoneUsers)
			{
				var discordUser = Client.GetUser(user.UserId);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			return embed.Build();
		}

		public static Embed MilestoneRebuildV2(DiscordSocketClient Client, OldActiveMilestone milestoneV2)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{milestoneV2.DateExpire.ToString("dd.MM.yyyy")}, {GlobalVariables.culture.DateTimeFormat.GetDayName(milestoneV2.DateExpire.DayOfWeek)} в {milestoneV2.DateExpire.ToString("HH:mm")} по МСК. {milestoneV2.Milestone.Type}: {milestoneV2.Milestone.Name}",
				Color = Color.Gold,
				ThumbnailUrl = milestoneV2.Milestone.Icon
			};
			if (milestoneV2.Milestone.PreviewDesc != null)
				embed.WithDescription(milestoneV2.Milestone.PreviewDesc);

			if (milestoneV2.Memo != null)
				embed.AddField("Заметка от лидера", milestoneV2.Memo);
			//Slot 1 (Raid Leader)
			embed.AddField("Страж #1", $"{Client.GetUser(milestoneV2.User1).Mention} - {Client.GetUser(milestoneV2.User1).Username}");
			//Slot 2
			if (milestoneV2.User2 != 0)
			{
				if (milestoneV2.User2 == milestoneV2.User1)
					embed.AddField("Страж #2", $"**Зарезервировано лидером.**");
				else
				{
					var user = Client.GetUser(milestoneV2.User2);
					embed.AddField("Страж #2", $"{user.Mention} - {user.Username}");
				}
			}
			else
				embed.AddField("Страж #2", "Свободно");
			//Slot 3
			if (milestoneV2.User3 != 0)
			{
				if (milestoneV2.User3 == milestoneV2.User1)
					embed.AddField("Страж #3", $"**Зарезервировано лидером.**");
				else
				{
					var user = Client.GetUser(milestoneV2.User3);
					embed.AddField("Страж #3", $"{user.Mention} - {user.Username}");
				}
			}
			else
				embed.AddField("Страж #3", "Свободно");
			//Slot 4
			if (milestoneV2.User4 != 0)
			{
				if (milestoneV2.User4 == milestoneV2.User1)
					embed.AddField("Страж #4", $"**Зарезервировано лидером.**");
				else
				{
					var user = Client.GetUser(milestoneV2.User4);
					embed.AddField("Страж #4", $"{user.Mention} - {user.Username}");
				}
			}
			else
				embed.AddField("Страж #4", "Свободно");
			//Slot 5
			if (milestoneV2.User5 != 0)
			{
				if (milestoneV2.User5 == milestoneV2.User1)
					embed.AddField("Страж #5", $"**Зарезервировано лидером.**");
				else
				{
					var user = Client.GetUser(milestoneV2.User5);
					embed.AddField("Страж #5", $"{user.Mention} - {user.Username}");
				}
			}
			else
				embed.AddField("Страж #5", "Свободно");
			//Slot 6
			if (milestoneV2.User6 != 0)
			{
				if (milestoneV2.User6 == milestoneV2.User1)
					embed.AddField("Страж #6", $"**Зарезервировано лидером.**");
				else
				{
					var user = Client.GetUser(milestoneV2.User6);
					embed.AddField("Страж #6", $"{user.Mention} - {user.Username}");
				}
			}
			else
				embed.AddField("Страж #6", "Свободно");

			embed.WithFooter("Чтобы за вами закрепили место нажмите на реакцию, соответствующую месту.");

			return embed.Build();
		}

		public static Embed MilestoneRemindInDM(SocketUser user, ActiveMilestone milestone, SocketGuild socketGuild)
		{
			var authorBuilder = new EmbedAuthorBuilder
			{
				Name = $"Доброго времени суток, {user.Username}",
				IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
			};

			var embed = new EmbedBuilder()
			{
				Title = $"Хочу вам напомнить, что у вас через 15 минут начнется {milestone.Milestone.Type.ToLower()}.",
				Author = authorBuilder,
				Color = Color.DarkMagenta,
				ThumbnailUrl = milestone.Milestone.Icon
			};
			if (milestone.Milestone.PreviewDesc != null)
				embed.WithDescription(milestone.Milestone.PreviewDesc);

			if (milestone.Memo != null)
				embed.AddField("Заметка от лидера:", milestone.Memo);
			embed.WithFooter($"{milestone.Milestone.Type}: {milestone.Milestone.Name}. Сервер: {socketGuild.Name}");

			return embed.Build();
		}
		#endregion

		#region Economic
		public static Embed Glimmer(Color color, string text, string title = null, EmbedFooterBuilder footerBuilder = null)
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
		#endregion

		#region Discord
		public static Embed GuildInfo(SocketCommandContext commandContext, UsersInStatuses users, EmbedFooterBuilder footer = null)
		{
			var cAt = commandContext.Guild.CreatedAt;

			var embed = new EmbedBuilder
			{
				Title = $"Инфо о сервере {commandContext.Guild.Name}",
				ThumbnailUrl = commandContext.Guild.IconUrl,
				Color = Color.Teal
			}
			.AddField("Создан", $"{cAt.Day}.{cAt.Month}.{cAt.Year}", true)
			.AddField("Регион", commandContext.Guild.VoiceRegionId, true)
			.AddField(GlobalVariables.InvisibleString, "*Онлайн статистика стражей*")
			.AddField("Всего", users.TotalUsers, true)
			.AddField("В сети", users.UsersOnline, true)
			.AddField("Не в сети", users.UsersOffline, true)
			.AddField("В голосовых каналах", users.UsersInvoice, true)
			.AddField("Не активен", users.UsersAFK, true)
			.AddField("Не беспокоить", users.UsersDnD, true)
			.AddField("В игре", users.UsersPlaying, true)
			.AddField("В Destiny 2", users.UsersInDestiny, true)
			.AddField(GlobalVariables.InvisibleString, "*Статистика сервера*")
			.AddField("Ролей", commandContext.Guild.Roles.Count, true)
			.AddField("Категорий", commandContext.Guild.CategoryChannels.Count, true)
			.AddField("Голосовых каналов", commandContext.Guild.VoiceChannels.Count, true)
			.AddField("Текстовых каналов", commandContext.Guild.TextChannels.Count, true);

			if (footer != null)
				embed.WithFooter(footer);

			return embed.Build();
		}

		public struct UsersInStatuses
		{
			public int TotalUsers;
			public int UsersOnline;
			public int UsersInvoice;
			public int UsersPlaying;
			public int UsersInDestiny;
			public int UsersOffline;
			public int UsersAFK;
			public int UsersDnD;
		}
		#endregion
	}
}