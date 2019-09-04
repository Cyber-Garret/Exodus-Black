using Bot.Models.Db.Destiny2;
using Bot.Modules.Administration;
using Bot.Preconditions;
using Bot.Services;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules.Commands
{
	[Cooldown(5)]
	[RequireBotPermission(ChannelPermission.SendMessages)]
	public class MainModule : BotModuleBase
	{
		#region Global fields
		readonly FailsafeContext db;
		readonly CommandService commandService;
		readonly DiscordSocketClient Client;
		readonly MilestoneService Milestone;
		#endregion
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

		public MainModule(FailsafeContext context, CommandService command, DiscordSocketClient socketClient, MilestoneService milestoneService)
		{
			db = context;
			commandService = command;
			Client = socketClient;
			Milestone = milestoneService;
		}

		[Command("справка")]
		[Summary("Основная справочная команда.")]
		public async Task MainHelp()
		{
			List<CommandInfo> commands = commandService.Commands.ToList();
			var guild = await db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == Context.Guild.Id);
			var app = await Client.GetApplicationInfoAsync();

			var mainCommands = string.Empty;
			var adminCommands = string.Empty;
			var musicCommands = string.Empty;
			foreach (CommandInfo command in commands)
			{
				if (command.Module.Name == typeof(MainModule).Name)
					mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";
				else if (command.Module.Name == typeof(ModerationModule).Name)
					adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";
				else if (command.Module.Name == typeof(MusicModule).Name)
					musicCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";

			}

			var embed = new EmbedBuilder()
				.WithColor(Color.Gold)
				.WithTitle($"Доброго времени суток. Меня зовут Нейроматрица, я ИИ \"Черного исхода\" адаптированный для Discord. Успешно функционирую с {app.CreatedAt.ToString("dd.MM.yyyy")}")
				.WithDescription(
				"Моя основная цель - своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
				"Также я могу предоставить информацию о экзотическом снаряжении,катализаторах.\n" +
				"Больше информации ты можешь найти в моей [группе ВК](https://vk.com/failsafe_bot)")
				.AddField("Основные команды", mainCommands.Substring(0, mainCommands.Length - 2))
				.AddField("Команды администраторов сервера", adminCommands.Substring(0, adminCommands.Length - 2))
				.AddField(Program.config.RadioModuleName, musicCommands.Substring(0, musicCommands.Length - 2));

			await ReplyAsync(embed: embed.Build());
		}

		[Command("инфо")]
		[Summary("Отображает полную информацию о команде.")]
		[Remarks("Пример: !инфо <команда>, например !инфо клан статус")]
		public async Task Info([Remainder] string searchCommand = null)
		{
			if (searchCommand == null)
			{
				await ReplyAndDeleteAsync(":x: Пожалуйста, введите полноcтью или частично команду о которой вы хотите получить справку.");
				return;
			}
			var command = commandService.Commands.Where(c => c.Name.IndexOf(searchCommand, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
			if (command == null)
			{
				await ReplyAndDeleteAsync($":x: Команда **{searchCommand}** не была найдена.");
				return;
			}
			var embed = new EmbedBuilder();

			var guild = await db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == Context.Guild.Id);

			embed.WithAuthor(Client.CurrentUser);
			embed.WithTitle($"Информация о команде: {command.Name}");
			embed.WithColor(Color.Gold);
			embed.WithDescription(command.Summary ?? "Команда без описания.");
			// First alias always command atribute
			if (command.Aliases.Count > 1)
			{
				string alias = string.Empty;
				//Skip command attribute
				foreach (var item in command.Aliases.Skip(1))
				{
					alias += $"{guild.CommandPrefix ?? "!"}{item}, ";
				}
				embed.AddField("Синонимы команды:", alias.Substring(0, alias.Length - 2));
			}
			if (command.Remarks != null)
				embed.AddField("Заметка:", command.Remarks);

			await ReplyAsync(embed: embed.Build());
		}

		[Command("зур")]
		[Summary("Отвечает в данный момент Зур в системе или нет, если да, то отображает ссылки на сторонние ресурсы с его расположением и ассортиментом.")]
		[Remarks("Пример: !зур, префикс команды может отличаться от стандартного.")]
		public async Task XurCommand()
		{
			#region Check DayOfWeek
			DateTime today = DateTime.Now;
			TimeSpan start = new TimeSpan(20, 0, 0);
			TimeSpan end = start; //setting the same value as your start and end time is same
			DateTime fridayStartCheck = new DateTime();
			DateTime tuesdayEndCheck = new DateTime();

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

			EmbedBuilder Embed = new EmbedBuilder()
				.WithThumbnailUrl("https://i.imgur.com/sFZZlwF.png");

			if (today >= fridayStartCheck && today <= tuesdayEndCheck)
			{

				Embed.WithColor(Color.Gold)
					.WithTitle($"Уважаемый пользователь {Context.User}")
					.WithDescription("По моим данным Зур в данный момент в пределах солнечной системы, но\n" +
					"так как мои алгоритмы глобального позиционирования пока еще в разработке, определить его точное местоположение я не могу.\n" +
					"[Но я уверена что тут ты сможешь отыскать его положение](https://whereisxur.com/)\n" +
					"[Или тут](https://ftw.in/game/destiny-2/find-xur)")
					.WithFooter("Напоминаю! Зур покинет пределы солнечной системы во вторник 20:00 по МСК. Это сообщение будет автоматически удалено через 2 минуты. ");

			}
			else
			{
				Embed.WithColor(Color.Red)
					.WithTitle($"Уважаемый пользователь {Context.User}")
					.WithDescription("По моим данным Зур не в пределах солнечной системы.")
					.WithFooter("Зур прибудет в пятницу 20:00 по МСК. Я сообщу. Это сообщение будет автоматически удалено через 2 минуты.");
			}
			await ReplyAndDeleteAsync(null, embed: Embed.Build(), timeout: TimeSpan.FromMinutes(2));
		}

		[Command("экзот")]
		[Summary("Отображает информацию о экзотическом снаряжении. Ищет как по полному названию, так и частичному.")]
		[Remarks("Пример: !экзот буря")]
		public async Task Exotic([Remainder]string Input = null)
		{
			#region Checks
			if (Input == null)
			{
				await ReplyAndDeleteAsync(":x: Пожалуйста, введите полное или частичное название экзотического снаряжения.");
				return;
			}

			Gear gear = null;

			//Get random Exotic gear
			if (Input.ToLower() == "любой")
			{
				Random r = new Random();
				int randomId = r.Next(1, db.Gears.Count());
				gear = db.Gears.AsNoTracking().Skip(randomId).Take(1).FirstOrDefault();
			}
			else
			{
				gear = db.Gears.AsNoTracking().Where(c => c.Name.IndexOf(Alias(Input), StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
			}
			//If not found gear by user input
			if (gear == null)
			{
				await ReplyAndDeleteAsync(":x: Этой информации в моей базе данных нет. :frowning:");
				return;
			}
			#endregion

			EmbedBuilder embed = new EmbedBuilder();

			embed.WithColor(Color.Gold);
			embed.WithTitle(gear.Type + " - " + gear.Name);//Exotic type and Name
			embed.WithThumbnailUrl(gear.IconUrl);//Icon
			embed.WithDescription(gear.Description);//Lore
			if (gear.isWeapon)//Only weapon can have catalyst
			{
				embed.AddField("Катализатор", gear.isHaveCatalyst == true ? "**Есть**" : "**Отсутствует**");
			}
			embed.AddField(gear.Perk, gear.PerkDescription, true);//Main Exotic perk
			if (gear.SecondPerk != null && gear.SecondPerkDescription != null)//Second perk if have.
				embed.AddField(gear.SecondPerk, gear.SecondPerkDescription, true);
			embed.AddField("Как получить:", gear.DropLocation, true);//How to obtain this exotic gear
			embed.WithImageUrl(gear.ImageUrl);//Exotic screenshot

			var app = await Context.Client.GetApplicationInfoAsync();//Get some bot info from discord api
			embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}.",
				"https://www.bungie.net/common/destiny2_content/icons/ee21b5bc72f9e48366c9addff163a187.png");


			await ReplyAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", embed: embed.Build());
		}

		[Command("катализатор")]
		[Summary("Отображает информацию о катализаторе для оружия.")]
		[Remarks("Пример: !катализатор мида")]
		public async Task GetCatalyst([Remainder]string Input = null)
		{
			var app = await Client.GetApplicationInfoAsync();
			var Embed = new EmbedBuilder();
			#region Checks
			//check user input
			if (Input == null)
			{
				Embed.WithAuthor("Добро пожаловать в базу данных катализаторов экзотического оружия Нейроматрицы");
				Embed.WithThumbnailUrl("https://bungie.net/common/destiny2_content/icons/d8acfda580e28f7765dd6a813394c847.png");
				Embed.WithDescription("Для того чтобы найти информацию о нужном тебе катализаторе теперь достаточно написать `!катализатор мида` и я сразу отображу что я о нем знаю.\n" +
					"А еще ты можешь написать `!катализатор любой` и я выдам тебе случайный катализатор.");
				Embed.AddField(Global.InvisibleString, "Полный список известных мне катализаторов ты можешь посмотреть [тут](https://vk.com/topic-184785875_40234113).");
				Embed.WithColor(Color.Blue);
				Embed.WithFooter($"Это сообщение будет автоматически удалено через 2 минуты.",
					@"https://bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg");

				await ReplyAndDeleteAsync(null, embed: Embed.Build(), timeout: TimeSpan.FromMinutes(2));
				return;
			}
			Catalyst catalyst = null;
			//Get random catalyst
			if (Input.ToLower() == "любой")
			{
				Random r = new Random();
				int randomId = r.Next(1, db.Catalysts.Count());
				catalyst = db.Catalysts.AsNoTracking().Skip(randomId).Take(1).FirstOrDefault();
			}
			else
			{
				catalyst = db.Catalysts.AsNoTracking().Where(c => c.WeaponName.IndexOf(Alias(Input), StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
			}
			//Если бд вернула null сообщаем пользователю что ничего не нашли.
			if (catalyst == null)
			{
				Embed.WithDescription(":x: Этой информации в моей базе данных нет. :frowning:");
				Embed.WithColor(Color.Red);
				Embed.AddField(Global.InvisibleString, "Полный список известных мне катализаторов ты можешь посмотреть [тут](https://vk.com/topic-184785875_40234113).");
				Embed.WithFooter("Это сообщение будет автоматически удалено через 1 минуту.");
				await ReplyAndDeleteAsync(null, embed: Embed.Build(), timeout: TimeSpan.FromMinutes(1));
				return;
			}
			#endregion

			Embed.WithTitle("Информация о катализаторе для оружия " + $"{catalyst.WeaponName}");
			Embed.WithColor(Color.Gold);
			if (!string.IsNullOrWhiteSpace(catalyst.Icon))
				Embed.WithThumbnailUrl(catalyst.Icon);
			if (!string.IsNullOrWhiteSpace(catalyst.Description))
				Embed.WithDescription(catalyst.Description);
			if (!string.IsNullOrWhiteSpace(catalyst.DropLocation))
				Embed.AddField("Как получить катализатор", catalyst.DropLocation);
			if (!string.IsNullOrWhiteSpace(catalyst.Masterwork))
				Embed.AddField("Задание катализатора", catalyst.Masterwork);
			if (!string.IsNullOrWhiteSpace(catalyst.Bonus))
				Embed.AddField("Бонус катализатор", catalyst.Bonus);
			Embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}#{app.Owner.Discriminator}.",
				@"https://bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg");

			await ReplyAsync($"Итак, {Context.User.Username}, вот что мне известно про этот катализатор.", embed: Embed.Build());
		}

		[Command("респект"), Alias("помощь", "донат")]
		[Summary("Информация о том как помочь развитию бота")]
		public async Task Donate()
		{
			var app = await Client.GetApplicationInfoAsync();

			var embed = new EmbedBuilder()
				.WithColor(Color.Green)
				.AddField("Patreon", "При помощи системы Патреон вы можете оформить месячную подписку или единоразово купить мне кофе на любую сумму.\n[Я на Patreon](https://www.patreon.com/Cyber_Garret)\nКофе от вас или подписка придаст мне, как разработчику, больше мотивации развивать бота и придавать ему больше возможностей и функций. И, как минимум, покрыть расходы на ежемесячную аренду сервера.")
				.WithImageUrl("https://cs6.pikabu.ru/images/previews_comm/2017-08_3/1502729121126769504.png")
				.WithFooter($"В любом случае спасибо что проявляете интерес к Нейроматрице. С наилучшими пожеланиями {app.Owner.Username}#{app.Owner.Discriminator}.");

			await ReplyAsync(embed: embed.Build());
		}

		[Command("сбор")]
		[Summary("Команда для анонса активностей.")]
		[Remarks("Пример: !сбор <Название> <Дата и время начала активности> <Заметка лидера(Не обязательно)>, например !сбор пн 17.07.2019-20:00 Тестовая заметка.\nВведите любой параметр команды неверно, и я отображу по нему справку.")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.EmbedLinks | ChannelPermission.ManageMessages | ChannelPermission.MentionEveryone)]
		[Cooldown(10)]
		public async Task RaidCollection(string milestoneName, int numPlaces, string raidTime, [Remainder]string userMemo = null)
		{
			var guild = await FailsafeDbOperations.GetGuildAccountAsync(Context.Guild.Id);

			var milestone = await db.Milestones.AsNoTracking().Where(r =>
				r.Name.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1 ||
				r.Alias.IndexOf(milestoneName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefaultAsync();

			if (milestone == null)
			{
				var AvailableRaids = "Доступные для регистрации активности:\n\n";
				var info = await db.Milestones.AsNoTracking().ToListAsync();

				foreach (var item in info)
				{
					AvailableRaids += $"**{item.Name}** или просто **{item.Alias}**\n";
				}

				var message = new EmbedBuilder()
					.WithTitle("Страж, я не разобрала в какую активность ты хочешь пойти")
					.WithColor(Color.Red)
					.WithDescription(AvailableRaids += "\nПример: !сбор пж 17.07.2019-20:00")
					.WithFooter("Хочу напомнить, что я ищу как по полному названию рейда так и частичному. Это сообщение будет автоматически удалено через 2 минуты.");
				await ReplyAndDeleteAsync(null, embed: message.Build(), timeout: TimeSpan.FromMinutes(2));
				return;
			}

			if (numPlaces < 1 || numPlaces > 6)
			{
				await ReplyAndDeleteAsync("Количество мест должно быть в диапазоне от 1 до 6");
				return;
			}

			DateTime.TryParseExact(raidTime, "dd.MM-HH:mm", CultureInfo.InstalledUICulture, DateTimeStyles.None, out DateTime dateTime);

			if (dateTime == new DateTime())
			{
				var message = new EmbedBuilder()
					.WithTitle("Страж, ты указал неизвестный мне формат времени")
					.WithColor(Color.Gold)
					.AddField("Я понимаю время начала рейда в таком формате",
					"Формат времени: **<день>.<месяц>.<год>-<час>:<минута>**\n" +
					"**День:** от 01 до 31\n" +
					"**Месяц:** от 01 до 12\n" +
					"**Час:** от 00 до 23\n" +
					"**Минута:** от 00 до 59\n" +
					"В итоге у тебя должно получиться: **05.07-20:05** Пример: !сбор пж 21.05-20:00")
					.AddField("Уведомление", "Время начала активности учитывается только по московскому времени. Также за 15 минут до начала активности, я уведомлю участников личным сообщением.")
					.WithFooter("Это сообщение будет автоматически удалено через 2 минуты.");
				await ReplyAndDeleteAsync(null, embed: message.Build(), timeout: TimeSpan.FromMinutes(2));
				return;
			}
			if (dateTime < DateTime.Now)
			{
				var message = new EmbedBuilder()
					.WithColor(Color.Red)
					.WithDescription($"Собрался в прошлое? Тебя ждет увлекательное шоу \"остаться в живых\" в исполнении моей команды Золотого Века. Не забудь попкорн\nБип...Удачи в {DateTime.Now.Year - 1000} г. и передай привет моему капитану.");
				await ReplyAndDeleteAsync(null, embed: message.Build());
				return;
			}

			var msg = await ReplyAsync(message: guild.GlobalMention, embed: Milestone.StartEmbed(Context.User, milestone, dateTime, userMemo).Build());
			await Milestone.RegisterMilestoneAsync(msg.Id, Context, numPlaces, milestone.Id, dateTime, userMemo);

			for (int i = 0; i < numPlaces; i++)
			{
				await msg.AddReactionAsync(Global.ReactPlaceNumber[$"{i + 2}"]);
			}
			//Slots
			//await msg.AddReactionAsync(Global.ReactPlaceNumber["2"]);
			//await msg.AddReactionAsync(Global.ReactPlaceNumber["3"]);
			//await msg.AddReactionAsync(Global.ReactPlaceNumber["4"]);
			//await msg.AddReactionAsync(Global.ReactPlaceNumber["5"]);
			//await msg.AddReactionAsync(Global.ReactPlaceNumber["6"]);

		}

		[Command("клан статус")]
		[Summary("Отображает отсортированный онлайн Destiny 2 клана, зарегистрированного в базе данных моим создателем.\n**Клан должен быть зарегистрирован моим создателем.**")]
		[Remarks("Пример: !клан статус <ID клана>, например !клан статус 3772661 или введите !клан статус без ID для отображения как добавить клан.")]
		public async Task GetGuildInfo(int GuildId = 0)
		{
			try
			{
				#region Checks
				if (GuildId == 0)
				{
					var app = await Context.Client.GetApplicationInfoAsync();
					var NotFoundMessage = new EmbedBuilder();
					NotFoundMessage.WithColor(Color.Gold);
					NotFoundMessage.WithTitle("Капитан, ты не указал ID гильдии.");
					NotFoundMessage.WithDescription(
						$"Чтобы узнать ID, достаточно открыть любой клан на сайте Bungie.\nНапример: <https://www.bungie.net/ru/ClanV2?groupid=3526561> и скопировать цифры после groupid=\n" +
						$"Синтаксис команды простой: **!клан статус 3526561**\n");
					NotFoundMessage.AddField("Кэш данных о клане Destiny 2",
						"Если ты желаешь, чтобы я начала обновлять и отображать актуальные данные о твоем клане,\n" +
						$"напиши моему создателю - {app.Owner.Username}#{app.Owner.Discriminator} или посети [Чёрный Исход](https://discordapp.com/invite/WcuNPM9)\n" +
						"Только так я могу оперативно отображать данные о твоих стражах.\n");
					NotFoundMessage.WithFooter("Это сообщение будет автоматически удалено через 2 минуты.");

					await ReplyAndDeleteAsync(null, embed: NotFoundMessage.Build(), timeout: TimeSpan.FromMinutes(2));
					return;
				}
				#endregion
				//Send calculating message because stastic forming near 30-50 sec.
				var message = await Context.Channel.SendMessageAsync("Это займет некоторое время.\nНачинаю проводить подсчет.");

				var destiny2Clan = db.Clans.AsNoTracking().Include(m => m.Members).FirstOrDefault(c => c.Id == GuildId);

				if (destiny2Clan == null)
				{
					await message.ModifyAsync(m => m.Content = ":x: Этой информации в моей базе данных нет. :frowning:\n" +
					"Если это ошибка сообщите моему создателю.\n" +
					"Если вы впервые воспользовались этой командой, напишите моему создателю, чтобы он добавил ваш клан в мою базу данных.");
					return;
				}


				var embed = new EmbedBuilder();
				embed.WithTitle($"Онлайн статус стражей клана `{destiny2Clan.Name}`");
				embed.WithColor(Color.Orange);
				////Bungie Clan link
				embed.WithUrl($"https://www.bungie.net/ru/ClanV2?groupid={GuildId}");
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
				embed.WithFooter($"Данные об онлайне стражей обновляються раз в час. Информация о клане и его составе раз в 15 минут.");

				//Modify old message and mention user with ready statistic
				await message.ModifyAsync(m =>
				{
					m.Content = $"Бип! {Context.User.Mention}. Статистика подсчитана.";
					m.Embed = embed.Build();
				});

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "GetGuildInfo", ex.Message, ex));
			}
		}
	}
}
