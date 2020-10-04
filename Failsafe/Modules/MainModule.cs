using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Preconditions;
using Failsafe.Properties;
using Failsafe.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neiralink;

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Failsafe.Modules
{
	[RequireContext(ContextType.Guild), Cooldown(5)]
	public class MainModule : RootModule
	{
		private readonly ILogger logger;
		private readonly DiscordSocketClient discord;
		private readonly CommandService command;
		private readonly EmoteService emote;
		private readonly IWishDbClient wishDb;
		public MainModule(IServiceProvider service, ILogger<MainModule> logger)
		{
			this.logger = logger;
			discord = service.GetRequiredService<DiscordSocketClient>();
			command = service.GetRequiredService<CommandService>();
			emote = service.GetRequiredService<EmoteService>();
			wishDb = service.GetRequiredService<IWishDbClient>();
		}

		#region Commands
		[Command("help"), Alias("справка", "довідка")]
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

		[Command("exotic"), Alias("экзот", "екзот")]
		public async Task Exotic([Remainder] string Input = null)
		{
			if (Input == null)
			{
				await ReplyAndDeleteAsync(Resources.ExoInputIsNull);
				return;
			}

			var exotic = ExoticData.SearchExotic(Input);
			//If not found gear by user input
			if (exotic == null)
			{
				await ReplyAndDeleteAsync(Resources.NotFoundInDB);
				return;
			}
			var embed = ExoticEmbed(exotic);

			await ReplyAsync(string.Format(Resources.ExoFound, Context.User.Username), embed: embed);
		}

		[Command("catalyst"), Alias("каталик", "каталік")]
		public async Task GetCatalyst([Remainder] string Input = null)
		{
			if (Input == null)
			{
				await ReplyAndDeleteAsync(Resources.CatInputIsNull);
				return;
			}

			var catalyst = CatalystData.Search(Input);

			//If not found gear by user input
			if (catalyst == null)
			{
				await ReplyAndDeleteAsync(Resources.NotFoundInDB);
				return;
			}
			var embed = CatalystEmbed(catalyst);

			await ReplyAsync(string.Format(Resources.CatFound, Context.User.Username), embed: embed);
		}

		[Command("mods"), Alias("моды", "моди")]
		public async Task ModsInfo()
		{
			try
			{
				await ReplyAsync(embed: ModsEmbed());
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Mod command");
			}
		}

		[Command("wish"), Alias("желание", "бажання")]
		public async Task Wishes()
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				await PagedReplyAsync(GetPaginatedWishesEmbed(guild.Language), new ReactionList { Trash = true, First = true, Last = true });
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Wish command");
			}
		}

		[Command("poll"), Alias("опрос", "опитування")]
		public async Task StartPoll([Remainder] string input)
		{
			var embed = PollEmbed(input, (SocketGuildUser)Context.User);

			var msg = await ReplyAsync(embed: embed);

			await msg.AddReactionsAsync(new IEmote[] { WhiteHeavyCheckMark, RedX });
		}

		[Command("bip"), Alias("бип", "біп")]
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

			//Sort all commands
			foreach (var command in commands)
			{
				//Main commands
				if (command.Module.Name == typeof(MainModule).Name)
				{
					if (guild.Language.Name == "en-US")
						mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
					else if (guild.Language.Name == "ru-RU")
						mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[1]}, ";
					else
						mainCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases.Last()}, ";
				}
				//Milestone commands
				else if (command.Module.Name == typeof(MilestoneModule).Name)
				{
					if (guild.Language.Name == "en-US")
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
					else if (guild.Language.Name == "ru-RU")
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[1]}, ";
					else
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases.Last()}, ";
				}
				//Admin commands
				else if (command.Module.Name == typeof(ModerationModule).Name)
				{
					if (guild.Language.Name == "en-US")
						adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
					else if (guild.Language.Name == "ru-RU")
						adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[1]}, ";
					else
						adminCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases.Last()}, ";
				}
				//Self role admin commands
				else if (command.Module.Name == typeof(SelfRoleModule).Name)
				{
					if (guild.Language.Name == "en-US")
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[0]}, ";
					else if (guild.Language.Name == "ru-RU")
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases[1]}, ";
					else
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{command.Aliases.Last()}, ";
				}
			}

			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.HelpEmbTitle, app.CreatedAt.Date.ToShortDateString()),
				Color = Color.Gold,
				Description = string.Format(Resources.HelpEmbDesc, Resources.NeiraWebSite),
				Footer = new EmbedFooterBuilder
				{
					IconUrl = Resources.NeiraFooterIcon,
					Text = Resources.MyAd
				}
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
					Text = $"{Resources.EmbFooterAboutMyMistake}\n{Resources.MyAd}"
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
					Text = $"{Resources.EmbFooterAboutMyMistake}\n{Resources.MyAd}"
				}
			}
			.AddField(Resources.CatEmbDrpFieldTitle, catalyst.DropLocation)
			.AddField(Resources.CatEmbQueFieldTitle, catalyst.Masterwork)
			.AddField(Resources.CatEmbBnsFieldTitle, catalyst.Bonus)
			.Build();

			return embed;
		}

		private Embed ModsEmbed()
		{
			var embed = new EmbedBuilder
			{
				Title = Resources.ModEmbTitle,
				Color = Color.Gold,
				Footer = new EmbedFooterBuilder { IconUrl = Resources.NeiraFooterIcon, Text = Resources.EmbFooterAboutMyMistake }
			};
			//Elements
			embed.AddField(GlobalVariables.InvisibleString, $"**{Resources.ModFieldElementTitle}**");
			embed.AddField($"{emote.Arc} {Resources.ModFieldArcTitle}", Resources.ModFieldArcDesc);
			embed.AddField($"{emote.Solar} {Resources.ModFieldSolarTitle}", Resources.ModFieldSolarDesc);
			embed.AddField($"{emote.Void} {Resources.ModFieldVoidTitle}", Resources.ModFieldVoidDesc);

			//Armor
			embed.AddField(GlobalVariables.InvisibleString, $"**{Resources.ModFieldTypeTitle}**");
			embed.AddField(Resources.ModFieldHelmetTitle, Resources.ModFieldHelmetDesc);
			embed.AddField(Resources.ModFieldGauntletsTitle, Resources.ModFieldGauntletsDesc);
			embed.AddField(Resources.ModFieldChestTitle, Resources.ModFieldChestDesc);
			embed.AddField(Resources.ModFieldLegTitle, Resources.ModFieldLegDesc);
			embed.AddField(Resources.ModFieldClassItemTitle, Resources.ModFieldClassItemDesc);

			return embed.Build();
		}

		private PaginatedMessage GetPaginatedWishesEmbed(CultureInfo guildLocale)
		{
			var localeISO = guildLocale.TwoLetterISOLanguageName.ToUpper();
			var wishes = wishDb.GetWishes(localeISO);

			var pager = new PaginatedMessage
			{
				Color = Color.DarkPurple,
				Pages = wishes.Select(wish => new Page { Title = wish.Title, Description = wish.Desc, ImageUrl = wish.WallScreenshot }),
				Options = { Timeout = TimeSpan.FromMinutes(5) },
				FooterTextOverride = Resources.WishFooterFormat
			};
			return pager;
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
				Color = Color.Green,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = Resources.NeiraFooterIcon,
					Text = Resources.MyAd
				}
			}
			.AddField(Resources.PollEmbTpcFieldTitle, text)
			.Build();

			return embed;
		}
		#endregion
	}
}
