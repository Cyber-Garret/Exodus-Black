using Core;
using Core.Models.Destiny2;

using Discord;
using Discord.Commands;

using DiscordBot.Features.Raid;
using DiscordBot.Modules.Administration;
using DiscordBot.Preconditions;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Commands
{
	[Cooldown(5)]
	[RequireBotPermission(ChannelPermission.SendMessages)]
	public class MainModule : BotModuleBase
	{
		#region Global fields
		readonly DateTime today = DateTime.Now;
		static TimeSpan start = new TimeSpan(20, 0, 0);
		static TimeSpan end = start; //setting the same value as your start and end time is same
		DateTime fridayStartCheck = new DateTime();
		DateTime tuesdayEndCheck = new DateTime();
		readonly FailsafeContext db;
		readonly CommandService commandService;
		#endregion
		#region Functions
		string Alias(string name)
		{
			if (name == "дарси")
				return "Д.А.Р.С.И.";
			else if (name == "мида")
				return "MIDA";
			else if (name == "сурос")
				return "SUROS";
			else if (name == "морозники")
				return "M0р03ники";
			else if (name == "топотуны")
				return "Т0п0тунЬI";
			else
				return name;
		}
		#endregion

		public MainModule(FailsafeContext context, CommandService command)
		{
			db = context;
			commandService = command;
		}

		[Command("справка")]
		[Summary("Основная справочная команда.")]
		public async Task MainHelp()
		{
			List<CommandInfo> commands = commandService.Commands.ToList();

			EmbedBuilder embedBuilder = new EmbedBuilder();
			var app = await Program.Client.GetApplicationInfoAsync();
			embedBuilder.WithColor(Color.Gold);
			embedBuilder.WithTitle($"Доброго времени суток. Меня зовут Нейроматрица, я ИИ \"Черного исхода\" адаптированный для Discord. Успешно функционирую с {app.CreatedAt.ToString("dd.MM.yyyy")}");
			embedBuilder.WithDescription(
				"Моя основная цель - своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
				"Также я могу предоставить информацию о экзотическом снаряжении,катализаторах.\n");

			string mainCommands = string.Empty;
			string adminCommands = string.Empty;
			foreach (CommandInfo command in commands)
			{
				if (command.Module.Name == typeof(MainModule).Name)
					mainCommands += $"!{command.Name}, ";
				else if (command.Module.Name == typeof(ModerationModule).Name)
					adminCommands += $"!{command.Name}, ";

			}

			embedBuilder.AddField("Основные команды", mainCommands.Substring(0, mainCommands.Length - 2));
			embedBuilder.AddField("Команды администраторов сервера", adminCommands.Substring(0, adminCommands.Length - 2));

			await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
		}

		[Command("инфо")]
		public async Task Info([Remainder] string searchCommand = null)
		{
			if (searchCommand == null)
			{
				await ReplyAsync(":x: Пожалуйста, введите полноcтью или частично команду ок которой вы хотите получить справку.");
				return;
			}
			var command = commandService.Commands.Where(c => c.Name.IndexOf(searchCommand, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
			if (command == null)
			{
				await ReplyAsync(":x: Пожалуйста, введите полноcтью или частично команду ок которой вы хотите получить справку.");
				return;
			}
			var embed = new EmbedBuilder();
			embed.WithAuthor(Program.Client.CurrentUser);
			embed.WithTitle($"Информация о команде: {command.Name}");
			embed.WithDescription(command.Summary ?? "Команда без описания.");

			if (command.Aliases.Count > 1)
			{
				string alias = string.Empty;

				foreach (var item in command.Aliases)
				{
					alias += $"!{item}, ";
				}
				embed.AddField("Синонимы команды:", alias.Substring(0, alias.Length - 2));
			}
			if (command.Remarks != null)
				embed.AddField("Пример команды:", command.Remarks);

			await ReplyAsync(embed: embed.Build());
		}

		[Command("зур")]
		[Summary("Отвечает в данный момент Зур в системе или нет, если да, то отображает ссылки на сторонние ресурсы с его расположением и ассортиментом.")]
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

		[Command("экзот")]
		public async Task Exotic([Remainder]string Input = null)
		{
			#region Checks
			//Проверяем ввел ли пользователь параметры для поиска.
			if (Input == null)
			{
				await Context.Channel.SendMessageAsync(":x: Пожалуйста, введите полное или частичное название экзотического снаряжения.");
				return;
			}
			//Запрашиваем информацию из локальной бд.
			Gear gear = FailsafeDbOperations.GetGears(Alias(Input));
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
				if (gear.isHaveCatalyst)
				{
					embed.AddField("Катализатор:", "**Есть**");
				}
				else
				{
					embed.AddField("Катализатор:", "**Отсутствует**");
				}
			}
			//Экзотическое свойство.
			embed.AddField(gear.Perk, gear.PerkDescription, true);
			//Второе уникальное или не очень свойство.
			if (gear.SecondPerk != null && gear.SecondPerkDescription != null)
				embed.AddField(gear.SecondPerk, gear.SecondPerkDescription, true);
			//Информация о том как получить снаряжение.
			embed.AddField("Как получить:", gear.DropLocation, true);
			//Скриншот снаряжения.
			embed.WithImageUrl(gear.ImageUrl);

			var app = await Context.Client.GetApplicationInfoAsync();
			embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}", "https://bungie.net/common/destiny2_content/icons/ee21b5bc72f9e48366c9addff163a187.png");


			await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, embed.Build());
		}

		[Command("катализатор")]
		[Summary("Выводит информацию об известных катализаторах.")]
		public async Task Catalyst([Remainder]string Input = null)
		{
			var app = await Program.Client.GetApplicationInfoAsync();
			var Embed = new EmbedBuilder();
			#region Checks
			//Проверяем ввел ли пользователь параметры для поиска.
			if (Input == null)
			{
				Embed.WithAuthor("Добро пожаловать в базу данных катализаторов экзотического оружия Нейроматрицы");
				Embed.WithThumbnailUrl("https://bungie.net/common/destiny2_content/icons/d8acfda580e28f7765dd6a813394c847.png");
				Embed.WithDescription("Для того чтобы найти информацию о нужном тебе катализаторе теперь достаточно написать `!катализатор мида` и я сразу отображу что я о нем знаю.\n" +
					"А еще ты можешь написать `!катализатор любой` и я выдам тебе случайный катализатор.");
				Embed.AddField(Global.InvisibleString, "Полный список известных мне катализаторов ты можешь посмотреть [тут](http://neira.link/Catalysts).");
				Embed.WithColor(Color.Blue);
				Embed.WithFooter($"Если нашли какие либо неточности или у вас есть предложения, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}", @"https://bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg");

				await Context.Channel.SendMessageAsync(null, false, Embed.Build());
				return;
			}
			//Запрашиваем информацию из локальной бд.
			var Catalyst = FailsafeDbOperations.GetCatalyst(Alias(Input));
			//Если бд вернула null сообщаем пользователю что ничего не нашли.
			if (Catalyst == null)
			{
				Embed.WithDescription(":x: Этой информации в моей базе данных нет. :frowning:");
				Embed.WithColor(Color.Red);
				Embed.AddField(Global.InvisibleString, "Полный список известных мне катализаторов ты можешь посмотреть [тут](http://neira.link/Catalysts).");
				await Context.Channel.SendMessageAsync(null, false, Embed.Build());
				return;
			}
			#endregion

			Embed.WithTitle("Информация о катализаторе для оружия " + $"{Catalyst.WeaponName}");
			Embed.WithColor(Color.Gold);
			if (!string.IsNullOrWhiteSpace(Catalyst.Icon))
				Embed.WithThumbnailUrl(Catalyst.Icon);
			if (!string.IsNullOrWhiteSpace(Catalyst.Description))
				Embed.WithDescription(Catalyst.Description);
			if (!string.IsNullOrWhiteSpace(Catalyst.DropLocation))
				Embed.AddField("Как получить катализатор", Catalyst.DropLocation);
			if (!string.IsNullOrWhiteSpace(Catalyst.Masterwork))
				Embed.AddField("Задание катализатора", Catalyst.Masterwork);
			if (!string.IsNullOrWhiteSpace(Catalyst.Bonus))
				Embed.AddField("Бонус катализатор", Catalyst.Bonus);
			Embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}", @"https://bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg");

			await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, Embed.Build());
		}

		[Command("респект"), Alias("помощь", "донат")]
		[Summary("Информация о том как помочь развитию бота")]
		public async Task Donate()
		{
			EmbedBuilder embed = new EmbedBuilder();
			var app = await Program.Client.GetApplicationInfoAsync();
			embed.WithColor(Color.Green);
			embed.AddField("Patreon", "При помощи системы Патреон вы можете оформить месячную подписку на любую сумму. [Подписка](https://www.patreon.com/Cyber_Garret) придаст мне, как разработчику, больше мотивации развивать бота и придавать ему больше возможностей и функций. И, как минимум, покрыть расходы на ежемесячную аренду сервера. А в будущем от подписки у тебя будет больше возможностей. :smiley: ");
			embed.WithFooter($"В любом случае спасибо что проявляете интерес к Нейроматрице. С наилучшими пожеланиями {app.Owner.Username}");

			await ReplyAsync(embed: embed.Build());
		}

		[Command("сбор")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.EmbedLinks | ChannelPermission.ManageMessages | ChannelPermission.MentionEveryone)]
		[Cooldown(10)]
		public async Task RaidCollection(string raidName, string raidTime, [Remainder]string userMemo = "Лидер не указал какие-либо особенности или требования.")
		{
			var raidInfo = await FailsafeDbOperations.GetMilestone(raidName);

			if (raidInfo == null)
			{
				var AvailableRaids = "Доступные для регистрации активности:\n\n";
				var info = await FailsafeDbOperations.GetAllMilestones();

				foreach (var item in info)
				{
					AvailableRaids += $"**{item.Name}** или просто **{item.Alias}**\n";
				}

				var message = new EmbedBuilder()
					.WithTitle("Страж, я не разобрала в какую активность ты хочешь пойти")
					.WithColor(Color.Red)
					.WithDescription(AvailableRaids += "\nПример: !сбор пж 17.07.2019-20:00")
					.WithFooter("Хочу напомнить, что я ищу как по полному названию рейда так и частичному.");
				await ReplyAsync(embed: message.Build());
				return;
			}

			DateTime.TryParseExact(raidTime, "dd.MM.yyyy-HH:mm", CultureInfo.InstalledUICulture, DateTimeStyles.None, out DateTime dateTime);

			if (dateTime == new DateTime())
			{
				var message = new EmbedBuilder()
					.WithTitle("Страж, ты указал неизвестный мне формат времени")
					.WithColor(Color.Gold)
					.AddField("Я понимаю время начала рейда в таком формате",
					"Формат времени: **<день>.<месяц>.<год>-<час>:<минута>**\n" +
					"**День:** от 01 до 31\n" +
					"**Месяц:** от 01 до 12\n" +
					"**Год:** Например: 2019\n" +
					"**Час:** от 00 до 23\n" +
					"**Минута:** от 00 до 59\n" +
					"В итоге у тебя должно получиться: **05.07.2019-20:05**")
					.AddField("Уведомление", "Время начала активности учитывается только по московскому времени. Также за 15 минут до начала активности, я уведомлю участников личным сообщением.")
					.WithFooter("Пример: !сбор пж 21.05.2018-20:00");
				await ReplyAsync(embed: message.Build());
				return;
			}
			if (dateTime < DateTime.Now)
			{
				var message = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithDescription($"Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи в {DateTime.Now.Year - 1000} г. и передай привет моему капитану.");
				await ReplyAsync(embed: message.Build());
				return;
			}

			//if (string.IsNullOrWhiteSpace(userMemo))
			//	userMemo = "**Рейд-лидер не указал какие-либо особенности или требования.**";

			var msg = await Context.Channel.SendMessageAsync(text: "@everyone", embed: RaidsCore.StartRaidEmbed(Context.User, raidInfo, dateTime, userMemo).Build());
			await RaidsCore.RegisterRaidAsync(msg.Id, Context.Guild.Name, Context.User.Id, raidInfo.Id, dateTime, userMemo);
			//Slots
			await msg.AddReactionAsync(RaidsCore.ReactOptions["2"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["3"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["4"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["5"]);
			await msg.AddReactionAsync(RaidsCore.ReactOptions["6"]);

		}

		[Command("клан статус")]
		[Summary("Возвращает результат онлайна соклановцев заданой гильдии")]
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
					var destiny2Clan = failsafe.Clans.AsNoTracking().Include(m => m.Members).ToList().FirstOrDefault(c => c.Id == GuildId);

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
				await Logger.Log(new LogMessage(LogSeverity.Error, Logger.GetExecutingMethodName(ex), ex.Message, ex));
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
