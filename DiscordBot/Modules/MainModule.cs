using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using DiscordBot.Preconditions;

using Core;
using Core.Models.Db;

namespace DiscordBot.Modules.Commands
{
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
		[Cooldown(10)]
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
		[Cooldown(10)]
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
			//Если в бд отмечено что оружие имеет катализатор добавляем несколько полей.
			if (gear.Catalyst)
			{
				embed.AddField("Катализатор:", "Есть");
				embed.AddField("Как получить катализатор:", gear.WhereCatalystDrop);
				embed.AddField("Задание катализатора:", gear.CatalystQuest);
				embed.AddField("Бонус катализатора:", gear.CatalystBonus);
			}
			else
			{
				embed.AddField("Катализатор:", "Отсутствует");
			}
			//Экзотическое свойство.
			embed.AddField(gear.PerkName, gear.PerkDescription, true);
			//Второе уникальное или не очень свойство.
			embed.AddField(gear.SecondPerkName, gear.SecondPerkDescription, true);
			//Информация о том как получить снаряжение.
			embed.AddField("Как получить:", gear.DropLocation, true);
			//Скриншот снаряжения.
			embed.WithImageUrl(gear.ImageUrl);

			var app = await Context.Client.GetApplicationInfoAsync();
			embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}", app.Owner.GetAvatarUrl());


			await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, embed.Build());
		}
	}
}
