using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Discord;
using Discord.Commands;

using DiscordBot.Preconditions;

using Core;
using Core.Models.Db;
using DiscordBot.Features.Catalyst;

namespace DiscordBot.Modules.Commands
{
	[Cooldown(5)]
	public class MainModule : BotModuleBase
	{
		#region Global fields
		readonly DateTime today = DateTime.Now;
		static TimeSpan start = new TimeSpan(20, 0, 0);
		static TimeSpan end = start; //setting the same value as your start and end time is same
		DateTime fridayStartCheck = new DateTime();
		DateTime tuesdayEndCheck = new DateTime();
		#endregion

		[Command("справка")]
		[Summary("Основная справочная команда.")]
		public async Task MainHelp()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.WithColor(Color.Gold);
			embedBuilder.WithTitle($"Доброго времени суток. Меня зовут Нейроматрица, я ИИ \"Черного исхода\" адаптированный для Discord");
			embedBuilder.WithDescription(
				"Моя основная цель своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
				"Так же я могу предоставить информацию о экзотическом вооружении.\n" +
				"В текущий момент в моей базе данных зарегистрированны такие команды:");
			embedBuilder.AddField(
				"Команда: **!инфо [название снаряжения]**",
				"Эта команда несет полную информацию о экзотическом вооружении представленном в игре.\n" +
				"Синтаксис довольно простой, можно искать как по полному названию так и частичному.\n" +
				"Например: **!инфо дело** предоставит информацию об автомате Милое Дело.");
			embedBuilder.AddField(
				"Команда: **!зур**",
				"Команда отображает находится ли в данный момент Зур в игре или нет.");
			embedBuilder.WithUrl("http://neira.link/");
			embedBuilder.WithFooter("Еще больше информации обо мне ты найдешь, нажав на заголовок этого сообщения.");

			await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
		}

		[Command("зур"), Summary("Информационная комманда о посланнике девяти по имени Зур")]
		
		public async Task XurCommand()
		{

			#region Check DayOfWeek
			if (today.DayOfWeek == DayOfWeek.Friday)
			{
				fridayStartCheck = DateTime.Parse(today.ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+5).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Saturday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-1).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+4).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Sunday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-2).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+3).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Monday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-3).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.AddDays(+2).ToShortDateString());
				tuesdayEndCheck += end;
			}
			if (today.DayOfWeek == DayOfWeek.Tuesday)
			{
				fridayStartCheck = DateTime.Parse(today.AddDays(-5).ToShortDateString());
				fridayStartCheck += start;
				tuesdayEndCheck = DateTime.Parse(today.ToShortDateString());
				tuesdayEndCheck += end;
			}
			#endregion

			if (today >= fridayStartCheck && today <= tuesdayEndCheck)
			{
				EmbedBuilder Embed = new EmbedBuilder()
					.WithColor(Color.Gold)
					.WithTitle($"Уважаемый пользователь {Context.User}")
					.WithThumbnailUrl("http://neira.link/img/Xur_emblem.png")
					.WithDescription("По моим данным Зур в данный момент в пределах солнечной системы, но\n" +
					"так как мои алгоритмы глобального позиционирования пока еще в разработке, определить точное положение я не могу.\n" +
					"[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
					"[Или тут](https://ftw.in/game/destiny-2/find-xur)")
					.WithFooter("Напоминаю! Зур покинет пределы солнечной системы во вторник 20:00 по МСК.");
				await Context.Channel.SendMessageAsync("", false, Embed.Build());
			}
			else
			{
				EmbedBuilder Embed = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithDescription("По моим данным Зур не в пределах солнечной системы.")
					.WithThumbnailUrl("http://neira.link/img/Xur_emblem.png")
					.WithFooter("Зур прибудет в пятницу 20:00 по МСК. Я сообщу.");
				await Context.Channel.SendMessageAsync("", false, Embed.Build());
			}
		}

		[Command("инфо")]
		[Cooldown(10)]
		public async Task GearInfo([Remainder]string Input = null)
		{
			#region Checks
			//Проверяем ввел ли пользователь параметры для поиска.
			if (Input == null)
			{
				await Context.Channel.SendMessageAsync(":x: Пожалуйста, введите полное или частичное название экзотического снаряжения.");
				return;
			}

			///Функция синонимов.
			string Alias()
			{
				if (Input == "дарси")
					return "Д.А.Р.С.И.";
				if (Input == "мида")
					return "MIDA";
				if (Input == "сурос")
					return "SUROS";
				if (Input == "морозники")
					return "M0р03ники";
				if (Input == "топотуны")
					return "Т0п0тунЬI";
				return Input;
			}
			//Запрашиваем информацию из локальной бд.
			Gear gear = FailsafeDbOperations.GetGears(Alias());
			//Если бд вернула null сообщаем пользователю что ничего не нашли.
			if (gear == null)
			{
				await Context.Channel.SendMessageAsync(":x: Этой информации в моей базе данных нет. :frowning:");
				return;
			}
			#endregion

			EmbedBuilder embed = new EmbedBuilder();

			embed.WithColor(Color.Gold);
			//Тип снаряжение и его имя.
			embed.WithTitle(gear.Type + " - " + gear.Name);
			//Иконка снаряжения.
			embed.WithThumbnailUrl(gear.IconUrl);
			//Краткая история снаряжения.
			embed.WithDescription(gear.Description);
			if (gear.isWeapon)
			{
				//Если в бд отмечено что оружие имеет катализатор добавляем несколько полей.
				if (gear.Catalyst)
				{
					embed.AddField("Катализатор:", "**Есть**");
				}
				else
				{
					embed.AddField("Катализатор:", "**Отсутствует**");
				}
			}
			//Экзотическое свойство.
			embed.AddField(gear.PerkName, gear.PerkDescription, true);
			//Второе уникальное или не очень свойство.
			if (gear.SecondPerkName != null && gear.SecondPerkDescription != null)
				embed.AddField(gear.SecondPerkName, gear.SecondPerkDescription, true);
			//Информация о том как получить снаряжение.
			embed.AddField("Как получить:", gear.DropLocation, true);
			//Скриншот снаряжения.
			embed.WithImageUrl(gear.ImageUrl);

			var app = await Context.Client.GetApplicationInfoAsync();
			embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}", "https://bungie.net/common/destiny2_content/icons/ee21b5bc72f9e48366c9addff163a187.png");


			await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, embed.Build());
		}


		[Command("катализатор"),
			Summary("Выводит информацию об известных катализаторах."),
			Cooldown(10),
			RequireContext(ContextType.Guild, ErrorMessage = "Прошу прощения страж, но данная команда не работает в личных сообщениях."),
			RequireBotPermission(ChannelPermission.AddReactions| ChannelPermission.SendMessages, ErrorMessage = "Прошу прощения, но я не могу выполнить эту команду пока не получу доступ на отправку сообщений и добавление реакций в этом канале.")]
		public async Task Catalyst()
		{
			var msg = await Context.Channel.SendMessageAsync("", false, CatalystData.CatalystStartingEmbed().Build());
			Global.CatalystMessages.Add(new CatalystCore(msg.Id, Context.User.Id));
			await msg.AddReactionAsync(CatalystData.ReactOptions["ok"]);
			await msg.AddReactionAsync(CatalystData.ReactOptions["1"]);
		}

		[Command("клан статус")]
		[Summary("Возвращает результат онлайна соклановцев заданой гильдии")]
		[Cooldown(5)]
		[RequireBotPermission(ChannelPermission.SendMessages)]
		public async Task GetGuildInfo(int GuildId = 0)
		{
			try
			{
				#region Checks
				if (GuildId == 0)
				{
					var app = await Context.Client.GetApplicationInfoAsync();
					EmbedBuilder embed = new EmbedBuilder();
					embed.WithColor(Color.Gold);
					embed.WithTitle("Капитан, ты не указал ID гильдии.");
					embed.WithDescription(
						$"Чтобы узнать ID, достаточно открыть любой клан на сайте Bungie.\nНапример: <https://www.bungie.net/ru/ClanV2?groupid=3526561> и скопировать цифры после groupid=\n" +
						$"Синтаксис команды простой: **!клан статус 3526561**\n");
					embed.AddField("Кэш данных о клане Destiny 2",
						"Если ты желаешь, чтобы я начала обновлять данные о клане которого нет [тут](http://neira.link/Clan).\n" +
						$"Напиши моему создателю - {app.Owner.Username}#{app.Owner.Discriminator} или посети [Чёрный Исход](https://discordapp.com/invite/WcuNPM9)\n" +
						"Только так я могу оперативно отображать данные о твоих стражах.\n");
					embed.WithFooter("Ты можешь посетить Черный исход по ссылке в описании и получать любую оперативную помощь и информацию от моего создателя.");

					await Context.Channel.SendMessageAsync(null, false, embed.Build());
					return;
				}
				#endregion
				//Send calculating message because stastic forming near 30-50 sec.
				var message = await Context.Channel.SendMessageAsync("Это займет некоторое время.\nНачинаю проводить подсчет.");

				using (var failsafe = new FailsafeContext())
				{
					var destiny2Clan = failsafe.Destiny2Clans.AsNoTracking().Include(m => m.Members).ToList().FirstOrDefault(c => c.Id == GuildId);

					if (destiny2Clan == null)
					{
						await Context.Channel.SendMessageAsync(":x: Этой информации в моей базе данных нет. :frowning:");
						return;
					}


					EmbedBuilder embed = new EmbedBuilder();
					embed.WithTitle($"Онлайн статус стражей клана {destiny2Clan.Name}");
					embed.WithColor(Color.Orange);
					////Bungie Clan link
					embed.WithUrl($"http://neira.link/Clan/Details/{GuildId}");
					////Some clan main info
					embed.WithDescription(
						$"В данный момент в клане **{destiny2Clan.MemberCount}**/100 стражей.\n" +
						$"Сортировка происходит от времени, когда вызвали данную команду.");

					#region list for member sorted for some days
					List<string> _ThisDay = new List<string>();
					List<string> _Yesterday = new List<string>();
					List<string> _ThisWeek = new List<string>();
					List<string> _MoreOneWeek = new List<string>();
					List<string> _NoData = new List<string>();
					#endregion

					//Main Sorting logic
					foreach (var member in destiny2Clan.Members)
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
					string ThisDay = string.Join(',', _ThisDay);
					if (!string.IsNullOrEmpty(ThisDay))
						embed.AddField("Был(a) сегодня", ThisDay);
					//Same as above, but who enter to the game yesterday
					string Yesterday = string.Join(',', _Yesterday);
					if (!string.IsNullOrEmpty(Yesterday))
						embed.AddField("Был(a) вчера", Yesterday);
					//Same as above, but who enter to the game more 5 days but fewer 7 days ago
					string ThisWeek = string.Join(',', _ThisWeek);
					if (!string.IsNullOrEmpty(ThisWeek))
						embed.AddField("Был(a) на этой неделе", ThisWeek);
					//Same as above, but who enter to the game more 7 days ago
					string MoreOneWeek = string.Join(',', _MoreOneWeek);
					if (!string.IsNullOrEmpty(MoreOneWeek))
						embed.AddField("Был(a) больше недели тому назад", MoreOneWeek);
					//For user who not have any data.
					string NoData = string.Join(',', _NoData);
					if (!string.IsNullOrEmpty(NoData))
						embed.AddField("Нет данных", NoData);
					//Simple footer with clan name
					embed.WithFooter($"Данные об онлайне стражей обновляються раз в 1 час.");
					//Mention user with ready statistic
					await Context.Channel.SendMessageAsync($"Бип! {Context.User.Mention}. Статистика подсчитана.", false, embed.Build());

					//Delete message from start this command
					await Context.Channel.DeleteMessageAsync(message);
				}
			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, $"ClanStatus Command - {ex.Source}", ex.Message, ex.InnerException));
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
