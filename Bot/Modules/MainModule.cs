using Bot.Core.Data;
using Bot.Models;
using Bot.Preconditions;
using Bot.Properties;
using Bot.Services;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
	[Cooldown(5)]
	public class MainModule : BaseModule
	{
		private readonly ILogger logger;
		private readonly IDataRepository data;
		private readonly DiscordSocketClient discord;
		private readonly CommandService command;
		private readonly EmoteService emote;
		public MainModule(IServiceProvider service, ILogger<MainModule> logger)
		{
			this.logger = logger;
			data = service.GetRequiredService<IDataRepository>();
			discord = service.GetRequiredService<DiscordSocketClient>();
			command = service.GetRequiredService<CommandService>();
			emote = service.GetRequiredService<EmoteService>();
		}

		#region Commands
		[Command("справка")]
		public async Task MainHelp()
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				var embed = await HelpEmbedAsync(guild);
				await ReplyAsync(embed: embed);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Help command");
			}
		}

		[Command("экзот")]
		[Summary("Отображает информацию о экзотическом снаряжении. Ищет как по полному названию, так и частичному.")]
		[Remarks("Пример: !экзот буря")]
		public async Task Exotic([Remainder]string Input = null)
		{
			if (Input == null)
			{
				await ReplyAndDeleteAsync(Resources.ExoInputIsNull);
				return;
			}

			var exotic = data.GetExotic(Input);
			//If not found gear by user input
			if (exotic == null)
			{
				await ReplyAndDeleteAsync(Resources.NotFoundInDB);
				return;
			}
			var embed = ExoticEmbed(exotic);

			await ReplyAsync(string.Format(Resources.ExoFound, Context.User.Username), embed: embed);
		}

		[Command("каталик")]
		[Summary("Отображает информацию о катализаторе для оружия.")]
		[Remarks("Пример: !катализатор мида или !каталик туз")]
		public async Task GetCatalyst([Remainder]string Input = null)
		{// TODO: Fix case insensitive
			if (Input == null)
			{
				await ReplyAndDeleteAsync(Resources.CatInputIsNull);
				return;
			}

			var catalyst = data.GetCatalyst(Input);

			//Если бд вернула null сообщаем пользователю что ничего не нашли.
			if (catalyst == null)
			{
				await ReplyAndDeleteAsync(Resources.NotFoundInDB);
				return;
			}
			var embed = CatalystEmbed(catalyst);

			await ReplyAsync(string.Format(Resources.CatFound, Context.User.Username), embed: embed);
		}

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
			await ReplyAsync(Resources.Bip);
		}
		#endregion

		#region Embeds
		private async Task<Embed> HelpEmbedAsync(Guild guild)
		{
			var app = await discord.GetApplicationInfoAsync();

			var mainCommands = string.Empty;
			var milestoneCommands = string.Empty;
			var adminCommands = string.Empty;
			var selfRoleCommands = string.Empty;

			var commands = command.Commands.ToList();

			foreach (var command in commands)
			{
				if (command.Module.Name == typeof(MainModule).Name)
					mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
				else if (command.Module.Name == typeof(MilestoneModule).Name)
					milestoneCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
				else if (command.Module.Name == typeof(ModerationModule).Name)
					adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
				else if (command.Module.Name == typeof(SelfRoleModule).Name)
					selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";

			}

			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.HelpEmbTitle, app.CreatedAt.Date),
				Color = Color.Gold,
				Description = string.Format(Resources.HelpEmbDesc, Resources.NeiraWebSite)
			}
			.AddField(Resources.HelpEmbMainFieldTitle, mainCommands[0..^2])
			.AddField(Resources.HelpEmbMilFieldTitle, milestoneCommands[0..^2])
			.AddField(Resources.HelpEmbAdmFieldTitle, adminCommands[0..^2])
			.AddField(Resources.HelpEmbSRolFieldTitle, selfRoleCommands[0..^2]);

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
					Text = Resources.EmbFooterAboutMyMistake
				}

			};
			if (exotic.isWeapon)//Only weapon can have catalyst field
			{
				embed.AddField(Resources.ExoEmbCatFieldTitle, exotic.isHaveCatalyst == true ? Resources.ExoEmbCatFieldDescYes : Resources.ExoEmbCatFieldDescNo);
			}
			embed.AddField(exotic.Perk, exotic.PerkDescription);//Main Exotic perk

			if (exotic.SecondPerk != null && exotic.SecondPerkDescription != null)//Second perk if have.
				embed.AddField(exotic.SecondPerk, exotic.SecondPerkDescription);

			embed.AddField(Resources.ExoEmbHowFieldTitle, exotic.DropLocation);

			return embed.Build();
		}

		private Embed CatalystEmbed(Catalyst catalyst)
		{
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.CatEmbTitle, catalyst.WeaponName),
				Color = Color.Gold,
				ThumbnailUrl = catalyst.Icon,
				Description = catalyst.Description,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = @"https://www.bungie.net/common/destiny2_content/icons/2caeb9d168a070bb0cf8142f5d755df7.jpg",
					Text = Resources.EmbFooterAboutMyMistake
				}
			}
			.AddField(Resources.CatEmbDrpFieldTitle, catalyst.DropLocation)
			.AddField(Resources.CatEmbQueFieldTitle, catalyst.Masterwork)
			.AddField(Resources.CatEmbBnsFieldTitle, catalyst.Bonus)
			.Build();

			return embed;
		}

		private Embed PollEmbed(string text, SocketGuildUser user)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = string.Format(Resources.PollEmbAuthorName, user.Nickname ?? user.Username),
					IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
				},
				Color = Color.Green
			}
			.AddField(Resources.PollEmbTpcFieldTitle, text)
			.Build();

			return embed;
		}
		#endregion
	}
}
