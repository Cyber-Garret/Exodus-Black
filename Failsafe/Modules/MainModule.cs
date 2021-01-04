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
		private readonly ILogger<MainModule> _logger;
		private readonly DiscordSocketClient _discord;
		private readonly CommandService _command;
		private readonly EmoteService _emote;
		private readonly IWishDbClient _wishDb;

		public MainModule(ILogger<MainModule> logger, DiscordSocketClient discord, CommandService command, EmoteService emote, IWishDbClient wishDb)
		{
			_logger = logger;
			_discord = discord;
			_command = command;
			_emote = emote;
			_wishDb = wishDb;
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
				_logger.LogError(ex, "Help command");
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
				_logger.LogError(ex, "Wish command");
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
			var app = await _discord.GetApplicationInfoAsync();

			var mainCommands = string.Empty;
			var milestoneCommands = string.Empty;
			var adminCommands = string.Empty;
			var selfRoleCommands = string.Empty;

			var commands = _command.Commands.ToList();

			//Sort all commands
			foreach (var commandInfo in commands)
			{
				switch (commandInfo.Module.Name)
				{
					//Main commands
					case nameof(MainModule) when guild.Language.Name == "en-US":
						mainCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[0]}, ";
						break;
					case nameof(MainModule) when guild.Language.Name == "ru-RU":
						mainCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[1]}, ";
						break;
					//Milestone commands
					case nameof(MainModule):
						mainCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[^1]}, ";
						break;
					case nameof(MilestoneModule) when guild.Language.Name == "en-US":
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[0]}, ";
						break;
					case nameof(MilestoneModule) when guild.Language.Name == "ru-RU":
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[1]}, ";
						break;
					//Admin commands
					case nameof(MilestoneModule):
						milestoneCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[^1]}, ";
						break;
					case nameof(ModerationModule) when guild.Language.Name == "en-US":
						adminCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[0]}, ";
						break;
					case nameof(ModerationModule) when guild.Language.Name == "ru-RU":
						adminCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[1]}, ";
						break;
					//Self role admin commands
					case nameof(ModerationModule):
						adminCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[^1]}, ";
						break;
					case nameof(SelfRoleModule) when guild.Language.Name == "en-US":
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[0]}, ";
						break;
					case nameof(SelfRoleModule) when guild.Language.Name == "ru-RU":
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[1]}, ";
						break;
					case nameof(SelfRoleModule):
						selfRoleCommands += $"{guild.CommandPrefix ?? "!"}{commandInfo.Aliases[^1]}, ";
						break;
				}
			}
			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.HelpEmbTitle, app.CreatedAt.Date.ToShortDateString()),
				Color = Color.Gold,
				Description = string.Format(Resources.HelpEmbDesc, Resources.NeiraWebSite)
			}
			.AddField(Resources.HelpEmbMainFieldTitle, mainCommands[..^2])
			.AddField(Resources.HelpEmbMilFieldTitle, milestoneCommands[..^2])
			.AddField(Resources.HelpEmbAdmFieldTitle, adminCommands[..^2])
			.AddField(Resources.HelpEmbSRolFieldTitle, selfRoleCommands[..^2]);

			return embed.Build();
		}

		private static Embed ExoticEmbed(Exotic exotic)
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
			if (exotic.IsWeapon)//Only weapon can have catalyst field
			{
				embed.AddField(Resources.ExoEmbCatFieldTitle, exotic.IsHaveCatalyst == true ? Resources.ExoEmbCatFieldDescYes : Resources.ExoEmbCatFieldDescNo);
			}
			embed.AddField(exotic.Perk, exotic.PerkDescription);//Main Exotic perk

			if (exotic.SecondPerk != null && exotic.SecondPerkDescription != null)//Second perk if have.
				embed.AddField(exotic.SecondPerk, exotic.SecondPerkDescription);

			embed.AddField(Resources.ExoEmbHowFieldTitle, exotic.DropLocation);

			return embed.Build();
		}

		private static Embed CatalystEmbed(Catalyst catalyst)
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

		private PaginatedMessage GetPaginatedWishesEmbed(CultureInfo guildLocale)
		{
			var localeIso = guildLocale.TwoLetterISOLanguageName.ToUpper();
			var wishes = _wishDb.GetWishes(localeIso);

			var pager = new PaginatedMessage
			{
				Color = Color.DarkPurple,
				Pages = wishes.Select(wish => new Page { Title = wish.Title, Description = wish.Desc, ImageUrl = wish.WallScreenshot }),
				Options = { Timeout = TimeSpan.FromMinutes(5) },
				FooterTextOverride = Resources.WishFooterFormat
			};
			return pager;
		}

		private static Embed PollEmbed(string text, SocketGuildUser user)
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
