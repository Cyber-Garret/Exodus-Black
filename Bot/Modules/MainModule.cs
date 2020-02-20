using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bot.Core.Data;
using System.Linq;
using Discord;
using Bot.Models;
using Bot.Services;

namespace Bot.Modules
{
	public class MainModule : BaseModule
	{
		private const string FooterTextAboutException = "Если нашли какие либо неточности, сообщите моему создателю: Cyber_Garret#5898";

		private readonly DiscordSocketClient discord;
		private readonly CommandService command;
		private readonly EmoteService emote;
		public MainModule(IServiceProvider service)
		{
			discord = service.GetRequiredService<DiscordSocketClient>();
			command = service.GetRequiredService<CommandService>();
			emote = service.GetRequiredService<EmoteService>();
		}

		#region Commands
		[Command("справка")]
		[Summary("Основная справочная команда.")]
		public async Task MainHelp()
		{
			var embed = await HelpEmbedAsync();
			await ReplyAsync(embed: embed);
		}

		[Command("экзот")]
		[Summary("Отображает информацию о экзотическом снаряжении. Ищет как по полному названию, так и частичному.")]
		[Remarks("Пример: !экзот буря")]
		public async Task Exotic([Remainder]string Input = null)
		{
			if (Input == null)
			{
				await ReplyAndDeleteAsync(":x: Пожалуйста, введите полное или частичное название экзотического снаряжения.");
				return;
			}

			var exotic = ExoticData.SearchExotic(Input);
			//If not found gear by user input
			if (exotic == null)
			{
				await ReplyAndDeleteAsync(":x: Этой информации в моей базе данных нет. :frowning:");
				return;
			}
			var embed = ExoticEmbed(exotic);

			await ReplyAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", embed: embed);
		}

		[Command("каталик"), Alias("катализатор")]
		[Summary("Отображает информацию о катализаторе для оружия.")]
		[Remarks("Пример: !катализатор мида или !каталик туз")]
		public async Task GetCatalyst([Remainder]string Input = null)
		{
			if (Input == null)
			{
				await ReplyAndDeleteAsync(":x: Пожалуйста, введите полное или частичное название экзотического оружия.");
				return;
			}

			var catalyst = CatalystData.SearchCatalyst(Input);

			//Если бд вернула null сообщаем пользователю что ничего не нашли.
			if (catalyst == null)
			{
				await ReplyAndDeleteAsync(":x: Этой информации в моей базе данных нет. :frowning:");
				return;
			}
			var embed = CatalystEmbed(catalyst);

			await ReplyAsync($"Итак, {Context.User.Username}, вот что мне известно про этот катализатор.", embed: embed);
		}

		[Command("моды")]
		[Summary("Подсказка о модификаторах в броню 2.0")]
		public async Task ModsInfo()
		{
			var embed = ModsEmbed();

			await ReplyAsync(embed: embed);
		}

		//TODO: Clan command

		[Command("опрос")]
		[Summary("Создает голосование среди стражей. Поддерживает разметку MarkDown.")]
		[Remarks("Синтаксис: !опрос <текст сообщение>\nПример: !опрос Добавляем 10 рейдовых каналов?")]
		public async Task StartPoll([Remainder] string input)
		{
			var embed = PollEmbed(input, (SocketGuildUser)Context.User);

			var msg = await ReplyAsync(embed: embed);

			await msg.AddReactionsAsync(new IEmote[] { WhiteHeavyCheckMark, RedX });
		}

		[Command("бип")]
		[Summary("Простая команда проверки моей работоспособности.")]
		public async Task Bip()
		{
			await ReplyAsync("Бип...");
		}
		#endregion

		#region Embeds
		private async Task<Embed> HelpEmbedAsync()
		{
			var app = await discord.GetApplicationInfoAsync();

			var mainCommands = string.Empty;
			var adminCommands = string.Empty;
			var selfRoleCommands = string.Empty;

			var guild = GuildData.GetGuildAccount(Context.Guild);

			foreach (var command in command.Commands.ToList())
			{
				if (command.Module.Name == typeof(MainModule).Name)
					mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";
				else if (command.Module.Name == typeof(ModerationModule).Name)
					adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";
				else if (command.Module.Name == typeof(SelfRoleModule).Name)
					selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{command.Name}, ";

			}

			var embed = new EmbedBuilder()
				.WithColor(Color.Gold)
				.WithTitle($"Доброго времени суток. Меня зовут Нейроматрица, я ИИ \"Черного исхода\" адаптированный для Discord. Успешно функционирую с {app.CreatedAt.ToString("dd.MM.yyyy")}")
				.WithDescription(
				"Моя основная цель - своевременно сообщать когда прибывает или улетает посланник девяти Зур.\n" +
				"Также я могу предоставить информацию о экзотическом снаряжении,катализаторах.\n" +
				"Больше информации ты можешь найти в моей [группе ВК](https://vk.com/failsafe_bot)\n" +
				"и в [документации](https://docs.neira.su/)")
				.AddField("Основные команды", mainCommands[0..^2])
				.AddField("Команды администраторов сервера", adminCommands[0..^2])
				.AddField("Команды настройки Автороли", selfRoleCommands[0..^2]);

			return embed.Build();
		}

		private Embed ExoticEmbed(Exotic exotic)
		{
			var embed = new EmbedBuilder
			{
				Title = $"{exotic.Type} - {exotic.Name}",
				Color = Color.Gold,
				ThumbnailUrl = exotic.IconUrl,
				Description = exotic.Description,
				ImageUrl = exotic.ImageUrl,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = @"https://www.bungie.net/common/destiny2_content/icons/ee21b5bc72f9e48366c9addff163a187.png",
					Text = FooterTextAboutException
				}

			};
			if (exotic.isWeapon)//Only weapon can have catalyst field
			{
				embed.AddField("Катализатор", exotic.isHaveCatalyst == true ? "**Есть**" : "**Отсутствует**");
			}
			embed.AddField(exotic.Perk, exotic.PerkDescription);//Main Exotic perk

			if (exotic.SecondPerk != null && exotic.SecondPerkDescription != null)//Second perk if have.
				embed.AddField(exotic.SecondPerk, exotic.SecondPerkDescription);

			embed.AddField("Как получить:", exotic.DropLocation);

			return embed.Build();
		}

		private Embed CatalystEmbed(Catalyst catalyst)
		{
			var embed = new EmbedBuilder
			{
				Title = "Информация о катализаторе для оружия " + $"{catalyst.WeaponName}",
				Color = Color.Gold,
				ThumbnailUrl = catalyst.Icon,
				Description = catalyst.Description,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = @"https://www.bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg",
					Text = FooterTextAboutException
				}
			}
			.AddField("Как получить катализатор", catalyst.DropLocation)
			.AddField("Задание катализатора", catalyst.Masterwork)
			.AddField("Бонус катализатор", catalyst.Bonus)
			.Build();

			return embed;
		}

		private Embed ModsEmbed()
		{
			var embed = new EmbedBuilder
			{
				Title = "Модификаторы брони 2.0",
				Color = Color.Gold,
				Footer = new EmbedFooterBuilder { Text = "neira.su", IconUrl = "http://neira.su/img/neira.png" }
			}
			//Elemens
			.AddField("Тип энергии к которому привязан тип оружия:", InvisibleString)
			.AddField($"{emote.Arc} Молния", "Импульсные винтовки, пулемёты, дробовики, мечи, луки.")
			.AddField($"{emote.Solar} Солнце", "Ракетные установки, автоматы, пистолеты-пулеметы, плазменные винтовки, линейно-плазменные винтовки.")
			.AddField($"{emote.Void} Пустота", "Револьверы, снайперские винтовки, гранатомёты, винтовки разведчиков, пистолеты")
			.AddField("Тип модификатора в доспехах", InvisibleString)
			//Armor Type
			.AddField("Шлем", "Прицельность, локаторы боеприпасов.")
			.AddField("Рукавицы", "Скорость перезарядки, откат гранат и ближнего боя.")
			.AddField("Нагрудник", "Резервный боезапас, стойкий прицел.")
			.AddField("Броня для ног", "Сборщик боеприпасов, легкость оружия.")
			.AddField("Классовый предмет", "Добивающий прием, восстановление способностей.")
			.Build();

			return embed;
		}

		private Embed PollEmbed(string text, SocketGuildUser user)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = $"Голосование от {user.Nickname ?? user.Username}",
					IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
				},
				Color = Color.Green
			}
			.AddField($"Тема голосования", text)
			.Build();

			return embed;
		}
		#endregion
	}
}
