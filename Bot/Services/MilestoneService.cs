using Bot.Core.Data;
using Bot.Models;
using Bot.Properties;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Services
{
	public class MilestoneService
	{
		private readonly ILogger _logger;
		private readonly DiscordShardedClient discord;
		private readonly EmoteService _emote;

		public MilestoneService(IServiceProvider service)
		{
			_logger = service.GetRequiredService<ILogger<MilestoneService>>();
			discord = service.GetRequiredService<DiscordShardedClient>();
			_emote = service.GetRequiredService<EmoteService>();
		}

		public async Task MilestoneReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = ActiveMilestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && !UserExist && milestone.MilestoneUsers.Count + 1 < milestone.MilestoneInfo.MaxSpace)
					{
						milestone.MilestoneUsers.Add(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);

						HandleReaction(msg, milestone);
					}
					else
					{
						var user = discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Added in Milestone");
			}
		}

		public async Task MilestoneReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
		{
			try
			{
				var msg = await cache.GetOrDownloadAsync();
				//get milestone
				var milestone = ActiveMilestoneData.GetMilestone(msg.Id);

				if (milestone == null) return;

				if (reaction.Emote.Equals(_emote.Raid))
				{
					//check reaction
					var UserExist = milestone.MilestoneUsers.Any(u => u == reaction.UserId);

					if (reaction.UserId != milestone.Leader && UserExist)
					{
						milestone.MilestoneUsers.Remove(reaction.UserId);
						ActiveMilestoneData.SaveMilestones(milestone.MessageId);
						HandleReaction(msg, milestone);
					}
					else
					{
						var user = discord.GetUser(reaction.UserId);
						await msg.RemoveReactionAsync(_emote.Raid, user);
					}
				}

			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Reaction Removed in Milestone");
			}
		}

		private async void HandleReaction(IUserMessage message, Milestone milestone)
		{
			var newEmbed = MilestoneEmbed(milestone);
			if (newEmbed.Length != 0)
				await message.ModifyAsync(m => m.Embed = newEmbed);
		}

		public async Task MilestoneNotificationAsync(Milestone milestone)
		{
			try
			{
				var loadedGuild = GuildData.GetGuildAccount(milestone.GuildId);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var RemindEmbed = this.RemindEmbed(milestone);

				var Leader = discord.GetUser(milestone.Leader);
				var LeaderDM = await Leader.GetOrCreateDMChannelAsync();

				await LeaderDM.SendMessageAsync(embed: RemindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{

						if (user == GlobalVariables.ReservedID) continue;

						var LoadedUser = discord.GetUser(user);

						var DM = await LoadedUser.GetOrCreateDMChannelAsync();
						await DM.SendMessageAsync(embed: RemindEmbed);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "RaidNotification in DM of user");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "RaidNotification Global");
			}

		}

		public async Task TimeChangedNotificationAsync(Milestone milestone)
		{
			try
			{
				var Guild = discord.GetGuild(milestone.GuildId);
				var loadedGuild = GuildData.GetGuildAccount(Guild);
				Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

				var channel = Guild.GetTextChannel(milestone.ChannelId);
				var msg = (IUserMessage)await channel.GetMessageAsync(milestone.MessageId);

				var RemindEmbed = TimeChangedEmbed(Guild, milestone, msg.GetJumpUrl());

				var Leader = discord.GetUser(milestone.Leader);
				var LeaderDM = await Leader.GetOrCreateDMChannelAsync();

				await LeaderDM.SendMessageAsync(embed: RemindEmbed);

				foreach (var user in milestone.MilestoneUsers)
				{
					try
					{

						if (user == GlobalVariables.ReservedID) continue;

						var LoadedUser = discord.GetUser(user);

						var DM = await LoadedUser.GetOrCreateDMChannelAsync();
						await DM.SendMessageAsync(embed: RemindEmbed);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "RaidNotification in DM of user");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "RaidNotification Global");
			}

		}

		public Embed MilestoneEmbed(Milestone milestone)
		{
			var loadedGuild = GuildData.GetGuildAccount(milestone.GuildId);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MilEmbTitle,
						  milestone.DateExpire.ToString("dd.MM.yyyy"),
						  milestone.DateExpire.ToString("HH:mm(zzz)"),
						  milestone.MilestoneInfo.Type,
						  milestone.MilestoneInfo.Name),
				Author = GetGame(milestone.MilestoneInfo.Game),
				ThumbnailUrl = milestone.MilestoneInfo.Icon,
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType)

			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			var leader = discord.GetUser(milestone.Leader);

			embed.AddField(Resources.MilEmbInfTitleField, string.Format(Resources.MilEmbInfDescField, leader.Mention, leader.Username, _emote.Raid));

			if (milestone.MilestoneUsers.Count > 0)
			{
				var embedFieldUsers = new EmbedFieldBuilder
				{
					Name = Resources.MilEmbMemTitleField
				};
				int count = 2;
				foreach (var user in milestone.MilestoneUsers)
				{
					if (user == GlobalVariables.ReservedID)
					{
						embedFieldUsers.Value += $"#{count} {Resources.MilReserved}\n";
					}
					else
					{
						var discordUser = discord.GetUser(user);
						embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";
					}
					count++;
				}

				embed.AddField(embedFieldUsers);
			}
			embed.WithFooter($"ID: {milestone.MessageId}\n{Resources.MyAd}");

			return embed.Build();
		}

		public Embed GetMilestonesNameEmbed(SocketGuild guild, MilestoneType type)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			var embed = new EmbedBuilder
			{
				Title = string.Format(Resources.MilInfEmbTitle, GetNameForMilestoneType(guild, type)),
				Color = GetColorByType(type)
			};
			if (type == MilestoneType.Raid)
			{
				var Des2names = string.Empty;
				var Div2names = string.Empty;
				foreach (var item in MilestoneInfoData.GetMilestonesByType(type))
				{
					if (item.Game == GameName.Destiny)
						Des2names += $"**{item.Alias}** - {item.Name}\n";
					else
						Div2names += $"**{item.Alias}** - {item.Name}\n";

				}
				embed.AddField("Destiny 2", Des2names);
				embed.AddField("The Division 2", Div2names);
			}
			else
			{
				var text = string.Empty;
				foreach (var milestone in MilestoneInfoData.GetMilestonesByType(type))
				{
					text += $"**{milestone.Alias}** - {milestone.Name}\n";
				}
				embed.Description = text;
			}
			embed.WithFooter($"{Resources.EmbFooterAboutDel}\n{Resources.MyAd}");

			return embed.Build();
		}

		private Embed RemindEmbed(Milestone milestone)
		{
			var guild = discord.GetGuild(milestone.GuildId);
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;


			var embed = new EmbedBuilder()
			{
				Title = string.Format(Resources.MilRemEmbTitle, milestone.MilestoneInfo.Type),
				Author = GetGame(milestone.MilestoneInfo.Game),
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType),
				ThumbnailUrl = milestone.MilestoneInfo.Icon
			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			var embedFieldUsers = new EmbedFieldBuilder
			{
				Name = Resources.MilEmbMemTitleField
			};
			var leader = discord.GetUser(milestone.Leader);
			embedFieldUsers.Value = $"#1 {leader.Mention} - {leader.Username}\n";
			int count = 2;
			foreach (var user in milestone.MilestoneUsers)
			{
				if (user == GlobalVariables.ReservedID) continue;

				var discordUser = discord.GetUser(user);
				embedFieldUsers.Value += $"#{count} {discordUser.Mention} - {discordUser.Username}\n";

				count++;
			}
			if (embedFieldUsers.Value != null)
				embed.AddField(embedFieldUsers);

			embed.WithFooter($"{string.Format(Resources.MilRemEmbFooter, milestone.MilestoneInfo.Type, milestone.MilestoneInfo.Name, guild.Name)}\n{Resources.MyAd}", guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}

		private Embed TimeChangedEmbed(SocketGuild guild, Milestone milestone, string jumpUrl)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			var embed = new EmbedBuilder()
			{
				Title = Resources.MilEmbTitleChangeTime,
				Author = GetGame(milestone.MilestoneInfo.Game),
				Color = GetColorByType(milestone.MilestoneInfo.MilestoneType),
				ThumbnailUrl = milestone.MilestoneInfo.Icon
			};
			if (milestone.Note != null)
				embed.WithDescription(string.Format(Resources.MilEmbDesc, milestone.Note));

			embed.AddField(Resources.MilEmbTimeFieldTitle, string.Format(Resources.MilEmbTimeFieldDesc, jumpUrl));

			embed.WithFooter($"{string.Format(Resources.MilRemEmbFooter, milestone.MilestoneInfo.Type, milestone.MilestoneInfo.Name, guild.Name)}\n{Resources.MyAd}", guild.IconUrl);
			embed.WithCurrentTimestamp();

			return embed.Build();
		}

		private Color GetColorByType(MilestoneType type)
		{
			return type switch
			{
				MilestoneType.Raid => Color.DarkMagenta,
				MilestoneType.Nightfall => Color.DarkGreen,
				MilestoneType.Other => Color.DarkBlue,
				_ => Color.Magenta,
			};
		}

		private string GetNameForMilestoneType(SocketGuild guild, MilestoneType type)
		{
			var loadedGuild = GuildData.GetGuildAccount(guild);
			Thread.CurrentThread.CurrentUICulture = loadedGuild.Language;

			return type switch
			{
				MilestoneType.Raid => Resources.Raid,
				MilestoneType.Nightfall => Resources.Nightfall,
				MilestoneType.Other => Resources.Other,
				_ => Resources.Unknown,
			};
		}

		private EmbedAuthorBuilder GetGame(GameName game)
		{
			var author = new EmbedAuthorBuilder();
			if (game == GameName.Destiny)
			{
				author.Name = "Destiny 2";
				author.IconUrl = @"https://www.neira.app/img/Destiny2.png";
				author.Url = @"https://www.bungie.net/";
			}
			else if (game == GameName.Division)
			{
				author.Name = "The Division 2";
				author.IconUrl = @"https://www.neira.app/img/Division2.png";
				author.Url = @"https://tomclancy-thedivision.ubisoft.com/game/";
			}
			else if (game == GameName.Warzone)
			{
				author.Name = "CoD: Warzone";
				author.IconUrl = @"https://www.neira.app/img/Warzone.png";
				author.Url = @"https://www.callofduty.com/warzone";
			}
			else
			{
				author.Name = "Unknown";
			}
			return author;
		}
	}
}